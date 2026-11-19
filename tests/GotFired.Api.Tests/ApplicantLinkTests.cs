using System.Net;
using GotFired.Api.Tests.Infrastructure;

namespace GotFired.Api.Tests;

/// <summary>Applicants reach their case only through the link emailed when staff answer.</summary>
public class ApplicantLinkTests(TestEnvironment env)
{
    private async Task<(int CaseId, string Name, string Email, string Link, ApiClient Admin)> AnsweredCaseAsync()
    {
        var (caseId, _, name, email) = await TestData.CreateCaseAsync(env);
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);
        Assert.Equal(HttpStatusCode.OK, (await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>")).Status);
        return (caseId, name, email, await env.Mail.LatestApplicantLinkTokenAsync(email), admin);
    }

    private static List<KeyValuePair<string, string>> ReplyForm(string link, int caseId, string text) =>
        [new("DismissalCaseId", caseId.ToString()), new("UserName", "Personel"), new("Guid", link), new("Text", text)];

    [Fact]
    public async Task Staff_answer_emails_the_applicant_on_file_a_link()
    {
        var (caseId, _, _, email) = await TestData.CreateCaseAsync(env);
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

        await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>", emailAddress: "attacker@example.com");

        var messages = await env.Mail.MessagesToAsync(email);
        Assert.Single(messages);
        Assert.Equal("İşten Atıldım Başvurunuz Hk.", (string)messages[0]["Subject"]);
        Assert.Empty(await env.Mail.MessagesToAsync("attacker@example.com"));
        string link = await env.Mail.LatestApplicantLinkTokenAsync(email);
        Assert.Equal(43, link.Length);   // 256-bit base64url
        Assert.Contains("https://gotfired.test/userview.html?guid=", await env.Mail.HtmlAsync(messages[0]));
    }

    [Fact]
    public async Task Emailed_link_shows_the_applicant_their_case()
    {
        var (_, name, _, link, _) = await AnsweredCaseAsync();

        var view = await env.CreateClient().GetAsync($"/api/v1/dismissalcase/userview/{link}");

        Assert.Equal(HttpStatusCode.OK, view.Status);
        Assert.Equal(name, (string)view["FullName"]);
        Assert.Equal("Kadın", (string)view["GenderName"]);
        Assert.Equal(link, (string)view["Guid"]);                     // userView.html posts it back with replies
        Assert.Contains(view["Comments"]!, c => (string)c["Text"] == "<p>cevap</p>");
    }

    [Fact]
    public async Task Applicant_page_does_not_reveal_staff_notes_and_is_not_cached()
    {
        var (caseId, _, _, link, admin) = await AnsweredCaseAsync();
        await admin.PostJsonAsync("/api/v1/dismissalcase/postNotes", new { notes = "iç not", dismissalCaseId = caseId.ToString() });

        var view = await env.CreateClient().GetAsync($"/api/v1/dismissalcase/userview/{link}");

        Assert.True(string.IsNullOrEmpty((string)view["Notes"]));
        Assert.Contains("no-store", view.Headers.CacheControl!.ToString());
        Assert.Equal("no-referrer", view.Headers.GetValues("Referrer-Policy").Single());
    }

    [Fact]
    public async Task Case_guid_does_not_open_the_applicant_page()
    {
        var (_, guid, _, _) = await TestData.CreateCaseAsync(env);

        var byCaseGuid = await env.CreateClient().GetAsync($"/api/v1/dismissalcase/userview/{guid}");
        var oldEmailedLink = await env.CreateClient().GetAsync("/api/v1/dismissalcase/userview/11111111-2222-3333-4444-555555555555");

        Assert.Equal(HttpStatusCode.BadRequest, byCaseGuid.Status);
        Assert.Equal(HttpStatusCode.BadRequest, oldEmailedLink.Status);
    }

    [Fact]
    public async Task Applicant_reply_goes_to_the_linked_case_under_the_applicants_name()
    {
        var (caseId, name, _, link, admin) = await AnsweredCaseAsync();
        int otherCase = 1;
        int otherBefore = await CommentCountAsync(otherCase);

        var reply = await env.CreateClient().PostFormAsync("/api/v1/dismissalcase/useranswer", ReplyForm(link, otherCase, "<p>soru</p>"));

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Equal(caseId, (int)reply["DismissalCaseID"]);
        Assert.Equal(name, (string)reply["UserName"]);
        Assert.Equal(otherBefore, await CommentCountAsync(otherCase));
        var pending = await admin.GetAsync("/api/v1/dismissalcase/pending/1");
        Assert.Contains(pending["data"]!, c => (int)c["Id"] == caseId);   // reply puts the case back in the queue
    }

    [Fact]
    public async Task Reply_without_a_valid_link_is_rejected()
    {
        int before = await CommentCountAsync(1);

        var response = await env.CreateClient().PostFormAsync("/api/v1/dismissalcase/useranswer",
            ReplyForm("11111111-2222-3333-4444-555555555555", 1, "spam"));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(before, await CommentCountAsync(1));
    }

    [Fact]
    public async Task A_new_staff_answer_replaces_the_previous_link()
    {
        var (caseId, _, email, firstLink, admin) = await AnsweredCaseAsync();
        await env.CreateClient().PostFormAsync("/api/v1/dismissalcase/useranswer", ReplyForm(firstLink, caseId, "<p>soru</p>"));

        await TestData.AnswerAsync(admin, caseId, "<p>ikinci cevap</p>");
        string secondLink = await env.Mail.LatestApplicantLinkTokenAsync(email);

        Assert.NotEqual(firstLink, secondLink);
        Assert.Equal(HttpStatusCode.BadRequest, (await env.CreateClient().GetAsync($"/api/v1/dismissalcase/userview/{firstLink}")).Status);
        var view = await env.CreateClient().GetAsync($"/api/v1/dismissalcase/userview/{secondLink}");
        Assert.Equal(["<p>cevap</p>", "<p>soru</p>", "<p>ikinci cevap</p>"], view["Comments"]!.Select(c => (string)c["Text"]));
    }

    [Fact]
    public async Task Tampered_link_is_rejected()
    {
        var (_, _, _, link, _) = await AnsweredCaseAsync();
        string tampered = link[..^1] + (link[^1] == 'A' ? 'B' : 'A');

        var response = await env.CreateClient().GetAsync($"/api/v1/dismissalcase/userview/{tampered}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    private Task<int> CommentCountAsync(int caseId) =>
        SqlScript.ScalarAsync<int>(env.LegacyConnectionString, $"SELECT COUNT(*) FROM Comments WHERE DismissalCaseID = {caseId}");
}
