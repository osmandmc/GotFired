using System.Net;
using GotFired.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace GotFired.Api.Tests;

/// <summary>Applicants reach their case only with the secret token from the email sent when staff answer.</summary>
public class ApplicantLinkTests(TestEnvironment env)
{
    private async Task<(int CaseId, string Name, string Email, string Token, ApiClient Admin)> AnsweredCaseAsync()
    {
        var (caseId, _, name, email) = await TestData.CreateCaseAsync(env);
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);
        Assert.Equal(HttpStatusCode.Created, (await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>")).Status);
        return (caseId, name, email, await env.Mail.LatestApplicantLinkTokenAsync(email), admin);
    }

    private Task<ApiResponse> GetMyCaseAsync(string token) =>
        env.CreateClient().GetAsync("/api/v1/applicant/case", TestData.ApplicantToken(token));

    private Task<ApiResponse> ReplyAsync(string token, string text) =>
        env.CreateClient().PostJsonAsync("/api/v1/applicant/case/comments", new { Text = text }, TestData.ApplicantToken(token));

    [Fact]
    public async Task Staff_answer_emails_the_applicant_a_link_with_the_token_in_the_fragment()
    {
        var (caseId, _, _, email) = await TestData.CreateCaseAsync(env);
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

        await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>");

        var message = Assert.Single(await env.Mail.MessagesToAsync(email));
        Assert.Equal("İşten Atıldım Başvurunuz Hk.", (string)message["Subject"]);
        // Fragment: never sent to the server. Exact file-name case: Linux is case-sensitive.
        Assert.Contains("https://gotfired.test/userView.html#token=", await env.Mail.HtmlAsync(message));
        Assert.Equal(43, (await env.Mail.LatestApplicantLinkTokenAsync(email)).Length);   // 256-bit base64url
    }

    [Fact]
    public async Task The_email_link_opens_an_existing_page()
    {
        var page = await env.CreateClient().GetAsync("/userView.html");

        Assert.Equal(HttpStatusCode.OK, page.Status);
    }

    [Fact]
    public async Task The_token_shows_the_applicant_their_case_without_staff_notes()
    {
        var (caseId, name, _, token, admin) = await AnsweredCaseAsync();
        await admin.PutJsonAsync($"/api/v1/cases/{caseId}/notes", new { Notes = "iç not" });

        var view = await GetMyCaseAsync(token);

        Assert.Equal(HttpStatusCode.OK, view.Status);
        Assert.Equal(name, (string)view["FullName"]);
        Assert.Equal("Kadın", (string)view["GenderName"]);
        Assert.Contains(view["Comments"]!, c => (string)c["Text"] == "<p>cevap</p>");
        Assert.Null(view["Notes"]);
        Assert.Null(view["Id"]);   // no internal ids for the applicant
        Assert.Contains("no-store", view.Headers.CacheControl!.ToString());
        Assert.Equal("no-referrer", view.Headers.GetValues("Referrer-Policy").Single());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("11111111-2222-3333-4444-555555555555")]   // an old case-Guid link
    public async Task Without_a_valid_token_the_case_does_not_exist(string token)
    {
        var response = await env.CreateClient().GetAsync("/api/v1/applicant/case", token == null ? null : TestData.ApplicantToken(token));

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task Applicant_reply_is_recorded_under_their_name_and_reopens_the_case()
    {
        var (caseId, name, _, token, admin) = await AnsweredCaseAsync();

        var reply = await ReplyAsync(token, "<p>soru</p>");

        Assert.Equal(HttpStatusCode.Created, reply.Status);
        Assert.Equal(name, (string)reply["UserName"]);
        Assert.Equal("<p>soru</p>", (string)reply["Text"]);
        var pending = await admin.GetAsync("/api/v1/cases?state=pending");
        Assert.Contains(pending["data"]!, c => (int)c["Id"] == caseId);
    }

    [Fact]
    public async Task Reply_without_a_valid_token_is_rejected()
    {
        int before = await SqlScript.ScalarAsync<int>(env.LegacyConnectionString, "SELECT COUNT(*) FROM Comments");

        var response = await ReplyAsync("11111111-2222-3333-4444-555555555555", "spam");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Equal(before, await SqlScript.ScalarAsync<int>(env.LegacyConnectionString, "SELECT COUNT(*) FROM Comments"));
    }

    [Fact]
    public async Task A_new_staff_answer_replaces_the_previous_token()
    {
        var (caseId, _, email, first, admin) = await AnsweredCaseAsync();
        await ReplyAsync(first, "<p>soru</p>");

        await TestData.AnswerAsync(admin, caseId, "<p>ikinci cevap</p>");
        string second = await env.Mail.LatestApplicantLinkTokenAsync(email);

        Assert.NotEqual(first, second);
        Assert.Equal(HttpStatusCode.NotFound, (await GetMyCaseAsync(first)).Status);
        var view = await GetMyCaseAsync(second);
        Assert.Equal(["<p>cevap</p>", "<p>soru</p>", "<p>ikinci cevap</p>"], view["Comments"]!.Select(c => (string)c["Text"]));
    }

    [Fact]
    public async Task Tampered_token_is_rejected()
    {
        var (_, _, _, token, _) = await AnsweredCaseAsync();
        string tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        Assert.Equal(HttpStatusCode.NotFound, (await GetMyCaseAsync(tampered)).Status);
    }

    [Fact]
    public async Task Token_expires_after_90_days()
    {
        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var (caseId, _, _, email) = await TestData.CreateCaseAsync(env);
        var admin = await env.CreateClient(s => s.AddSingleton<TimeProvider>(clock)).LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);
        await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>");
        string token = await env.Mail.LatestApplicantLinkTokenAsync(email);
        var applicant = env.CreateClient(s => s.AddSingleton<TimeProvider>(clock));

        clock.Advance(TimeSpan.FromDays(89));
        var stillValid = await applicant.GetAsync("/api/v1/applicant/case", TestData.ApplicantToken(token));
        clock.Advance(TimeSpan.FromDays(2));
        var expired = await applicant.GetAsync("/api/v1/applicant/case", TestData.ApplicantToken(token));

        Assert.Equal(HttpStatusCode.OK, stillValid.Status);
        Assert.Equal(HttpStatusCode.NotFound, expired.Status);
    }
}
