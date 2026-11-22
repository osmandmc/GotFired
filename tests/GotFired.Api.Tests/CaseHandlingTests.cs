using System.Net;
using GotFired.Api.Tests.Infrastructure;

namespace GotFired.Api.Tests;

/// <summary>What staff do with a case in the admin SPA.</summary>
public class CaseHandlingTests(TestEnvironment env)
{
    private Task<ApiClient> AdminAsync() => env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

    [Theory]
    [InlineData("")]
    [InlineData("?state=pending")]
    [InlineData("?state=answered&page=1")]
    public async Task Case_lists_are_paged_by_20(string query)
    {
        var response = await (await AdminAsync()).GetAsync($"/api/v1/cases{query}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(20, (int)response["paging"]!["ItemsPerPage"]);
        Assert.True(response["data"]!.Count() <= 20);
    }

    [Fact]
    public async Task An_unknown_state_filter_is_a_validation_problem()
    {
        var response = await (await AdminAsync()).GetAsync("/api/v1/cases?state=closed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotNull(response["errors"]!["state"]);
    }

    [Fact]
    public async Task Case_view_shows_city_supporters_and_conversation()
    {
        var view = await (await AdminAsync()).GetAsync("/api/v1/cases/1");   // legacy fixture case

        Assert.Equal(HttpStatusCode.OK, view.Status);
        Assert.Equal("İstanbul", (string)view["CityName"]);
        Assert.Equal(["Sendika"], view["SupportedByNames"]!.Select(s => (string)s));
        Assert.Contains(view["Comments"]!, c => (string)c["Text"] == "İlk mesaj");
    }

    [Fact]
    public async Task Case_view_shows_the_SGK_reason_the_applicant_chose()
    {
        var anonymous = env.CreateClient();
        var form = await anonymous.GetAsync("/api/v1/application-form");
        var sgk = form["sgkTerminationReasons"]![0]!;
        string name = TestData.Unique("SGK");
        var fields = TestData.ApplicationForm((string)form["guid"], name, "sgk@example.com", (int)form["cities"]![0]!["ID"])
            .Where(f => f.Key != "SGKTerminationReasonID")
            .Append(new("SGKTerminationReasonID", ((int)sgk["ID"]).ToString()))
            .ToList();
        await anonymous.PostFormAsync("/api/v1/applications", fields);
        var admin = await AdminAsync();

        var view = await admin.GetAsync($"/api/v1/cases/{await TestData.FindCaseIdAsync(admin, name)}");

        Assert.Equal($"{sgk["Code"]} - {sgk["Name"]}", (string)view["SGKTerminationReasonName"]);
    }

    [Fact]
    public async Task A_missing_case_is_404_problem_details()
    {
        var response = await (await AdminAsync()).GetAsync("/api/v1/cases/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Equal("application/problem+json", response.ContentHeaders.ContentType?.MediaType);
        Assert.Equal(404, (int)response["status"]);
    }

    [Fact]
    public async Task Staff_answer_is_201_recorded_under_the_signed_in_user_and_moves_the_case_to_answered()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        var answer = await TestData.AnswerAsync(admin, caseId, "<p>cevap</p>");

        Assert.Equal(HttpStatusCode.Created, answer.Status);
        Assert.Equal($"/api/v1/cases/{caseId}", answer.Location);
        Assert.Equal(TestEnvironment.AdminUser, (string)answer["UserName"]);
        var answered = await admin.GetAsync("/api/v1/cases?state=answered");
        Assert.Contains(answered["data"]!, c => (int)c["Id"] == caseId);
    }

    [Fact]
    public async Task Notes_are_replaced_with_PUT()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        var response = await admin.PutJsonAsync($"/api/v1/cases/{caseId}/notes", new { Notes = "not" });

        Assert.Equal(HttpStatusCode.NoContent, response.Status);
        Assert.Equal("not", (string)(await admin.GetAsync($"/api/v1/cases/{caseId}"))["Notes"]);
    }

    [Fact]
    public async Task Changing_a_missing_case_is_404()
    {
        var response = await (await AdminAsync()).PutJsonAsync("/api/v1/cases/999999/notes", new { Notes = "x" });

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task Tags_are_replaced_with_PUT_on_the_collection()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();
        int tagId = (int)(await admin.GetAsync("/api/v1/tags?search=taz")).Json!.Single(t => (string)t["Name"] == "tazminat")["ID"];

        var response = await admin.PutJsonAsync($"/api/v1/cases/{caseId}/tags", new { TagIds = new[] { tagId } });

        Assert.Equal(HttpStatusCode.NoContent, response.Status);
        var view = await admin.GetAsync($"/api/v1/cases/{caseId}");
        Assert.Equal([tagId], view["DismissalCaseTags"]!.Select(t => (int)t["ID"]));
    }

    [Fact]
    public async Task A_single_tag_is_added_with_idempotent_PUT_and_removed_with_DELETE()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();
        int tagId = (int)(await admin.GetAsync("/api/v1/tags")).Json!.First()["ID"];

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsync($"/api/v1/cases/{caseId}/tags/{tagId}")).Status);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsync($"/api/v1/cases/{caseId}/tags/{tagId}")).Status);
        var afterAdd = await admin.GetAsync($"/api/v1/cases/{caseId}");
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/v1/cases/{caseId}/tags/{tagId}")).Status);
        var afterRemove = await admin.GetAsync($"/api/v1/cases/{caseId}");

        Assert.Equal([tagId], afterAdd["DismissalCaseTags"]!.Select(t => (int)t["ID"]));
        Assert.Empty(afterRemove["DismissalCaseTags"]!);
    }

    [Fact]
    public async Task Marking_a_case_answered_sets_its_appeal_state()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        var response = await admin.PutJsonAsync($"/api/v1/cases/{caseId}/appeal-state", new { AppealState = 2 });

        Assert.Equal(HttpStatusCode.NoContent, response.Status);
        Assert.Equal(2, (int)(await admin.GetAsync($"/api/v1/cases/{caseId}"))["AppealState"]);
    }

    [Fact]
    public async Task Staff_cannot_set_a_case_back_to_pending()
    {
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);

        var response = await (await AdminAsync()).PutJsonAsync($"/api/v1/cases/{caseId}/appeal-state", new { AppealState = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task Concurrent_changes_to_one_case_all_succeed()
    {
        // e.g. an answer still being saved while the same staff member marks the case read (found by the UI test).
        var (caseId, _, _, _) = await TestData.CreateCaseAsync(env);
        var admin = await AdminAsync();

        var responses = await Task.WhenAll(Enumerable.Range(0, 6).Select(i => (i % 3) switch
        {
            0 => TestData.AnswerAsync(admin, caseId, $"<p>cevap {i}</p>"),
            1 => admin.PutJsonAsync($"/api/v1/cases/{caseId}/notes", new { Notes = $"not {i}" }),
            _ => admin.PutJsonAsync($"/api/v1/cases/{caseId}/appeal-state", new { AppealState = 2 }),
        }));

        Assert.All(responses, r => Assert.True(r.Status is HttpStatusCode.Created or HttpStatusCode.NoContent, $"{r.Status}: {r.Body}"));
        var view = await admin.GetAsync($"/api/v1/cases/{caseId}");
        Assert.Equal(2, view["Comments"]!.Count());
    }

    [Fact]
    public async Task Templates_and_categories_are_offered_while_answering()
    {
        var editor = await AdminAsync();

        var templates = await editor.GetAsync("/api/v1/answer-templates?pageSize=100");
        var template = await editor.GetAsync($"/api/v1/answer-templates/{(int)templates["data"]![0]!["ID"]}");
        var categories = await editor.GetAsync("/api/v1/categories");

        Assert.False(string.IsNullOrEmpty((string)template["Content"]));
        Assert.Equal(HttpStatusCode.OK, categories.Status);
    }
}
