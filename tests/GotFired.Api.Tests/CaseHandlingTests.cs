using System.Net;
using GotFired.Api.Tests.Infrastructure;

namespace GotFired.Api.Tests;

/// <summary>What staff do with a case in the admin SPA.</summary>
public class CaseHandlingTests(TestEnvironment env)
{
    private Task<ApiClient> AdminAsync() => env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

    [Theory]
    [InlineData("index/1")]
    [InlineData("pending/1")]
    [InlineData("answered/1")]
    public async Task Case_lists_are_paged_by_20(string path)
    {
        var response = await (await AdminAsync()).GetAsync($"/api/v1/dismissalcase/{path}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(20, (int)response["paging"]!["ItemsPerPage"]);
        Assert.True(response["data"]!.Count() <= 20);
    }

    [Fact]
    public async Task Case_view_shows_city_supporters_and_conversation()
    {
        var view = await (await AdminAsync()).GetAsync("/api/v1/dismissalcase/view/1");   // legacy fixture case

        Assert.Equal(HttpStatusCode.OK, view.Status);
        Assert.Equal("İstanbul", (string)view["CityName"]);
        Assert.Equal(["Sendika"], view["SupportedByNames"]!.Select(s => (string)s));
        Assert.Contains(view["Comments"]!, c => (string)c["Text"] == "İlk mesaj");
    }

    [Fact]
    public async Task Case_view_shows_the_SGK_reason_the_applicant_chose()
    {
        var anonymous = env.CreateClient();
        var lookups = await anonymous.GetAsync("/api/v1/dismissalcase/apply");
        var sgk = lookups["sgkTerminationReasons"]![0]!;
        string name = TestData.Unique("SGK");
        var form = TestData.ApplicationForm((string)lookups["guid"], name, "sgk@example.com", (int)lookups["cities"]![0]!["ID"])
            .Where(f => f.Key != "SGKTerminationReasonID")
            .Append(new("SGKTerminationReasonID", ((int)sgk["ID"]).ToString()))   // and no declared reason
            .ToList();
        await anonymous.PostFormAsync("/api/v1/dismissalcase/apply", form);
        var admin = await AdminAsync();

        var view = await admin.GetAsync($"/api/v1/dismissalcase/view/{await TestData.FindCaseIdAsync(admin, name)}");

        Assert.Equal((int)sgk["ID"], (int)view["SGKTerminationReasonId"]);
        Assert.Equal($"{sgk["Code"]} - {sgk["Name"]}", (string)view["SGKTerminationReasonName"]);
    }

    [Fact]
    public async Task Missing_case_returns_the_old_error_message()
    {
        var response = await (await AdminAsync()).GetAsync("/api/v1/dismissalcase/view/999999");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("model cannot be null", (string)response["Message"]);
    }

    [Fact]
    public async Task Staff_answer_is_recorded_under_the_signed_in_user_and_moves_the_case_to_answered()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>");

        var view = await admin.GetAsync($"/api/v1/dismissalcase/view/{caseId}");
        Assert.Equal([TestEnvironment.AdminUser], view["Comments"]!.Select(c => (string)c["UserName"]));
        var answered = await admin.GetAsync("/api/v1/dismissalcase/answered/1");
        Assert.Contains(answered["data"]!, c => (int)c["Id"] == caseId);
    }

    [Fact]
    public async Task Notes_are_saved()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        // The SPA sends camelCase keys and the id as a string.
        var response = await admin.PostJsonAsync("/api/v1/dismissalcase/postNotes", new { notes = "not", dismissalCaseId = caseId.ToString() });

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("not", (string)(await admin.GetAsync($"/api/v1/dismissalcase/view/{caseId}"))["Notes"]);
    }

    [Fact]
    public async Task Evaluating_a_case_replaces_its_tags()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();
        var tags = await admin.GetAsync("/api/v1/dismissalcase/getTags/taz");
        int tagId = (int)tags.Json!.Single(t => (string)t["Name"] == "tazminat")["ID"];

        await admin.PostJsonAsync("/api/v1/dismissalcase/evaluate", new { DismissalCaseId = caseId, CategoryId = 0, Tags = new[] { new { ID = tagId, Name = "tazminat" } } });

        var view = await admin.GetAsync($"/api/v1/dismissalcase/view/{caseId}");
        Assert.Equal([tagId], view["DismissalCaseTags"]!.Select(t => (int)t["ID"]));
    }

    [Fact]
    public async Task Tags_can_be_removed_and_added_one_at_a_time()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();
        int tagId = (int)(await admin.GetAsync("/api/v1/dismissalcase/getTags/taz")).Json!.First()["ID"];

        await admin.PostJsonAsync("/api/v1/dismissalcase/addTag", new { DismissalCaseID = caseId, TagID = tagId });
        var afterAdd = await admin.GetAsync($"/api/v1/dismissalcase/view/{caseId}");
        await admin.PostJsonAsync("/api/v1/dismissalcase/removeTag", new { DismissalCaseID = caseId, TagID = tagId });
        var afterRemove = await admin.GetAsync($"/api/v1/dismissalcase/view/{caseId}");

        Assert.Equal([tagId], afterAdd["DismissalCaseTags"]!.Select(t => (int)t["ID"]));
        Assert.Empty(afterRemove["DismissalCaseTags"]!);
    }

    [Fact]
    public async Task Mark_as_read_moves_a_pending_case_to_answered()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        var response = await admin.PostJsonAsync("/api/v1/dismissalcase/makeRead", new { Id = caseId });

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains((await admin.GetAsync("/api/v1/dismissalcase/answered/1"))["data"]!, c => (int)c["Id"] == caseId);
    }

    [Fact]
    public async Task Answer_templates_and_categories_are_offered_while_answering()
    {
        var admin = await AdminAsync();

        var templates = await admin.GetAsync("/api/v1/dismissalcase/getAnswerTemplates");
        var content = await admin.GetAsync($"/api/v1/dismissalcase/getAnswerTemplateContent/{(int)templates.Json![0]!["ID"]}");
        var categories = await admin.GetAsync("/api/v1/dismissalcase/getCategories");

        Assert.NotNull(templates.Json[0]!["Name"]);
        Assert.False(string.IsNullOrEmpty((string)content["Content"]));
        Assert.Equal(HttpStatusCode.OK, categories.Status);
    }
}
