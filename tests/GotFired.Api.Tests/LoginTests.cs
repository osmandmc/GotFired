using System.Net;
using GotFired.Api.Tests.Infrastructure;

namespace GotFired.Api.Tests;

public class LoginTests(TestEnvironment env)
{
    [Fact]
    public async Task Staff_can_log_in_with_a_password_from_the_old_system()
    {
        string user = TestData.Unique("v2-user");
        await SqlScript.RunAsync(env.LegacyConnectionString, $"""
            INSERT AspNetUsers (Id, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed, TwoFactorEnabled,
                                LockoutEnabled, AccessFailedCount, UserName, NormalizedUserName, ConcurrencyStamp)
            VALUES (NEWID(), 0, '{IdentityV2Hash("eski-parola")}', NEWID(), 0, 0, 0, 0, '{user}', UPPER('{user}'), NEWID());
            """);

        var response = await env.CreateClient().RequestTokenAsync(user, "eski-parola");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        string hash = await SqlScript.ScalarAsync<string>(env.LegacyConnectionString, $"SELECT PasswordHash FROM AspNetUsers WHERE UserName = '{user}'");
        Assert.StartsWith("AQAAAA", hash);   // rehashed from Identity v2 to v3 on first login
    }

    /// <summary>ASP.NET Identity 2 format: 0x00 | salt(16) | PBKDF2-HMAC-SHA1(1000 iterations, 32 bytes).</summary>
    private static string IdentityV2Hash(string password)
    {
        byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        byte[] subkey = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(password, salt, 1000, System.Security.Cryptography.HashAlgorithmName.SHA1, 32);
        return Convert.ToBase64String([0, .. salt, .. subkey]);
    }

    [Fact]
    public async Task Login_response_has_the_fields_login_html_stores()
    {
        var response = await env.CreateClient().RequestTokenAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.False(string.IsNullOrEmpty((string)response["access_token"]));
        Assert.Equal("bearer", (string)response["token_type"]);
        Assert.Equal(86400, (int)response["expires_in"]);
        Assert.Equal(TestEnvironment.AdminUser, (string)response["userName"]);
    }

    [Fact]
    public async Task Wrong_password_is_rejected_as_invalid_grant()
    {
        var response = await env.CreateClient().RequestTokenAsync(TestEnvironment.AdminUser, "wrong");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("invalid_grant", (string)response["error"]);
    }

    [Fact]
    public async Task Unknown_user_is_rejected_without_an_error()
    {
        var response = await env.CreateClient().RequestTokenAsync("no-such-user", "x");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("invalid_grant", (string)response["error"]);
    }
}

public class AuthorizationTests(TestEnvironment env)
{
    [Fact]
    public async Task Staff_resources_require_a_login()
    {
        var response = await env.CreateClient().GetAsync("/api/v1/cases");

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
    }

    [Fact]
    public async Task A_plain_user_cannot_use_admin_only_actions()
    {
        var user = await env.CreateClient().LoginAsync(TestEnvironment.PlainUser, TestEnvironment.LegacyPassword);

        var response = await user.PostJsonAsync("/api/v1/users", new { UserName = TestData.Unique("x"), Password = "abcdef" });

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }

    [Fact]
    public async Task Anonymous_self_registration_no_longer_exists()
    {
        var response = await env.CreateClient().PostJsonAsync("/api/v1/auth/register", new { UserName = TestData.Unique("x"), Password = "abcdef" });

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }
}

public class ApplicationFormTests(TestEnvironment env)
{
    [Fact]
    public async Task Form_lookups_use_turkish_display_names()
    {
        var response = await env.CreateClient().GetAsync("/api/v1/application-form");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.False(string.IsNullOrEmpty((string)response["guid"]));
        var city = response["cities"]![0]!;
        Assert.NotNull(city["ID"]);
        Assert.NotNull(city["Name"]);
        Assert.Contains(response["genders"]!, g => (string)g["Name"] == "Kadın");
        Assert.NotEmpty(response["supportedBys"]!);
        Assert.NotNull(response["sgkTerminationReasons"]![0]!["Code"]);
    }

    [Fact]
    public async Task Submitted_application_is_accepted_and_appears_as_a_pending_case()
    {
        var (caseId, _, name, _) = await TestData.CreateCaseAsync(env);
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

        var pending = await admin.GetAsync("/api/v1/cases?state=pending");

        Assert.Contains(pending["data"]!, c => (int)c["Id"] == caseId && (string)c["ApplicantFullName"] == name);
    }

    [Fact]
    public async Task Double_clicked_submit_stores_one_case()
    {
        var anonymous = env.CreateClient();
        var form = await anonymous.GetAsync("/api/v1/application-form");
        string name = TestData.Unique("Çift");
        var fields = TestData.ApplicationForm((string)form["guid"], name, "double@example.com", (int)form["cities"]![0]!["ID"]);

        var first = await anonymous.PostFormAsync("/api/v1/applications", fields);
        var second = await anonymous.PostFormAsync("/api/v1/applications", fields);

        Assert.Equal(HttpStatusCode.Accepted, first.Status);
        Assert.Equal(HttpStatusCode.Accepted, second.Status);
        int count = await SqlScript.ScalarAsync<int>(env.LegacyConnectionString,
            $"SELECT COUNT(*) FROM DismissalCases d JOIN Applicants a ON a.ID = d.ApplicantId WHERE a.FullName = N'{name}'");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Invalid_application_is_a_validation_problem_naming_the_field()
    {
        var response = await env.CreateClient().PostFormAsync("/api/v1/applications",
            [new("Guid", Guid.NewGuid().ToString()), new("ApplicantEmail", "")]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("application/problem+json", response.ContentHeaders.ContentType?.MediaType);
        Assert.NotEmpty(response["errors"]!["ApplicantEmail"]!);
    }

    [Fact]
    public async Task An_empty_form_lists_every_missing_field_in_turkish()
    {
        var response = await env.CreateClient().PostFormAsync("/api/v1/applications",
            [new("ApplicantEmail", ""), new("ApplicantGender", ""), new("CityID", ""), new("DismissalState", ""), new("EmployeeCount", ""), new("EmploymentDurationSince", "")]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        var errors = response["errors"]!;
        Assert.Equal("Cinsiyet seçin.", (string)errors["ApplicantGender"]![0]);
        Assert.Equal("Şehir seçin.", (string)errors["CityID"]![0]);
        Assert.NotNull(errors["DismissalState"]);
        Assert.NotNull(errors["EmployeeCount"]);
        Assert.NotNull(errors["EmploymentDurationSince"]);
        Assert.NotNull(errors["ApplicantEmail"]);
    }
}
