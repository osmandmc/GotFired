using System.Net;
using GotFired.Api.Tests.Infrastructure;

namespace GotFired.Api.Tests;

public class AnswerTemplateTests(TestEnvironment env)
{
    private Task<ApiClient> AdminAsync() => env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

    private static async Task<(string Location, ApiResponse Created)> CreateAsync(ApiClient admin)
    {
        var created = await admin.PostJsonAsync("/api/v1/answer-templates", new { Name = TestData.Unique("Şablon"), Content = "<p>x</p>" });
        Assert.Equal(HttpStatusCode.Created, created.Status);
        return (created.Location, created);
    }

    private static Dictionary<string, string> IfMatch(string etag) => new() { ["If-Match"] = etag };

    [Fact]
    public async Task Creating_a_template_returns_201_with_its_location()
    {
        var admin = await AdminAsync();

        var (location, created) = await CreateAsync(admin);

        Assert.Equal($"/api/v1/answer-templates/{(int)created["ID"]}", location);
        Assert.Equal("<p>x</p>", (string)(await admin.GetAsync(location))["Content"]);
    }

    [Fact]
    public async Task A_template_is_updated_with_PUT_and_the_ETag_from_GET()
    {
        var admin = await AdminAsync();
        var (location, created) = await CreateAsync(admin);
        var current = await admin.GetAsync(location);

        var update = await admin.PutJsonAsync(location, new { Name = (string)created["Name"], Content = "<p>y</p>" }, IfMatch(current.ETag));

        Assert.Equal(HttpStatusCode.NoContent, update.Status);
        var after = await admin.GetAsync(location);
        Assert.Equal("<p>y</p>", (string)after["Content"]);
        Assert.NotEqual(current.ETag, after.ETag);   // a new version
    }

    [Fact]
    public async Task Saving_with_an_outdated_ETag_is_412_and_changes_nothing()
    {
        var admin = await AdminAsync();
        var (location, created) = await CreateAsync(admin);
        string etag = (await admin.GetAsync(location)).ETag;
        await admin.PutJsonAsync(location, new { Name = (string)created["Name"], Content = "<p>theirs</p>" }, IfMatch(etag));

        var stale = await admin.PutJsonAsync(location, new { Name = (string)created["Name"], Content = "<p>mine</p>" }, IfMatch(etag));

        Assert.Equal(HttpStatusCode.PreconditionFailed, stale.Status);
        Assert.StartsWith("Başka bir kullanıcı güncelleme yapmakta", (string)stale["title"]);
        Assert.Equal("<p>theirs</p>", (string)(await admin.GetAsync(location))["Content"]);
    }

    [Fact]
    public async Task Updating_without_If_Match_is_428()
    {
        var admin = await AdminAsync();
        var (location, created) = await CreateAsync(admin);

        var response = await admin.PutJsonAsync(location, new { Name = (string)created["Name"], Content = "<p>y</p>" });

        Assert.Equal((HttpStatusCode)428, response.Status);
    }

    [Fact]
    public async Task Only_admins_change_templates()
    {
        var user = await env.CreateClient().LoginAsync(TestEnvironment.PlainUser, TestEnvironment.LegacyPassword);

        var response = await user.PostJsonAsync("/api/v1/answer-templates", new { Name = "x", Content = "y" });

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}

public class UserManagementTests(TestEnvironment env)
{
    private Task<ApiClient> AdminAsync() => env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

    [Fact]
    public async Task User_list_shows_role_names()
    {
        var list = await (await AdminAsync()).GetAsync("/api/v1/users");

        Assert.True((int)list["paging"]!["TotalItems"] >= 2);
        Assert.Contains(list["data"]!, u => (string)u["UserName"] == TestEnvironment.AdminUser && u["RoleNames"]!.Select(r => (string)r).SequenceEqual(["admin"]));
    }

    [Fact]
    public async Task Admin_can_create_a_user_assign_a_role_and_set_the_password()
    {
        var admin = await AdminAsync();
        string userName = TestData.Unique("personel");

        var create = await admin.PostJsonAsync("/api/v1/users", new { UserName = userName, Password = "abcdef", FriendlyName = "Personel" });
        var update = await admin.PutJsonAsync(create.Location, new { FriendlyName = "Personel 2", UserRoles = new[] { "editor" } });
        var password = await admin.PutJsonAsync($"{create.Location}/password", new { Password = "yenisi" });

        Assert.Equal(HttpStatusCode.Created, create.Status);
        Assert.Equal(HttpStatusCode.NoContent, update.Status);
        Assert.Equal(HttpStatusCode.NoContent, password.Status);
        var user = await admin.GetAsync(create.Location);
        Assert.Equal(["editor"], user["UserRoles"]!.Select(r => (string)r));
        Assert.Equal("Personel 2", (string)user["FriendlyName"]);
        var editor = await env.CreateClient().LoginAsync(userName, "yenisi");
        Assert.Equal(HttpStatusCode.OK, (await editor.GetAsync("/api/v1/cases")).Status);
    }

    [Fact]
    public async Task A_too_short_password_is_422_with_the_reason_and_the_old_password_keeps_working()
    {
        var admin = await AdminAsync();
        string userName = TestData.Unique("personel");
        var create = await admin.PostJsonAsync("/api/v1/users", new { UserName = userName, Password = "abcdef" });

        var response = await admin.PutJsonAsync($"{create.Location}/password", new { Password = "abc" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Equal("Kullanıcı parolası 6 karakterden az olamaz.", (string)response["detail"]);
        Assert.Equal(HttpStatusCode.OK, (await env.CreateClient().RequestTokenAsync(userName, "abcdef")).Status);
    }

    [Fact]
    public async Task An_unknown_user_is_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await (await AdminAsync()).GetAsync("/api/v1/users/no-such-id")).Status);
    }

    [Fact]
    public async Task All_roles_are_listed()
    {
        var roles = await (await AdminAsync()).GetAsync("/api/v1/roles");

        Assert.Superset(new HashSet<string> { "admin", "editor", "user" }, roles.Json!.Select(r => (string)r).ToHashSet());
    }
}

public class ErrorHandlingTests(TestEnvironment env)
{
    [Fact]
    public async Task Unexpected_errors_return_500_problem_details_and_email_the_error_recipient()
    {
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);
        int before = await ErrorEmailsForAsync("/tags/{tagId:int}");

        // A tag id that does not exist violates the foreign key: an unexpected database error.
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var response = await admin.PutAsync($"/api/v1/cases/{caseId}/tags/999999");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal("application/problem+json", response.ContentHeaders.ContentType?.MediaType);
        Assert.Equal(before + 1, await ErrorEmailsForAsync("/tags/{tagId:int}"));
    }

    // Other tests run in parallel and may trigger error emails too: count only this route's.
    private async Task<int> ErrorEmailsForAsync(string route)
    {
        int count = 0;
        foreach (var message in await env.Mail.MessagesToAsync("errors@tests.local"))
            if ((await env.Mail.HtmlAsync(message)).Contains(route)) count++;
        return count;
    }
}

public class SchemaTests(TestEnvironment env)
{
    /// <summary>
    /// Production after the runbook (EF6 schema + upgrade script + migrations) must be the schema EF Core expects,
    /// i.e. what the migrations create on an empty database. One column is kept on purpose for rollback.
    /// </summary>
    [Fact]
    public async Task Upgraded_legacy_database_matches_a_database_created_by_the_migrations()
    {
        string snapshot = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "db/tests/schema-snapshot.sql"), TestContext.Current.CancellationToken);

        var legacy = await SqlScript.QueryLinesAsync(env.LegacyConnectionString, snapshot);
        var fresh = await SqlScript.QueryLinesAsync(env.FreshConnectionString, snapshot);

        Assert.Equal(["COL AspNetUsers.LockoutEndDateUtc datetime NULL"], legacy.Except(fresh));
        Assert.Empty(fresh.Except(legacy));
    }
}
