using System.Net;
using GotFired.Api.Tests.Infrastructure;

namespace GotFired.Api.Tests;

public class AnswerTemplateTests(TestEnvironment env)
{
    private Task<ApiClient> AdminAsync() => env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

    private static async Task<int> FindTemplateIdAsync(ApiClient admin, string name)
    {
        for (int page = 1; ; page++)
        {
            var list = await admin.GetAsync($"/api/v1/answerTemplate/list/{page}");
            if (!list["data"]!.Any()) throw new InvalidOperationException($"Template {name} not found.");
            var match = list["data"]!.FirstOrDefault(t => (string)t["Name"] == name);
            if (match != null) return (int)match["ID"];
        }
    }

    [Fact]
    public async Task Admin_can_create_and_edit_a_template()
    {
        var admin = await AdminAsync();
        string name = TestData.Unique("Şablon");

        Assert.Equal(HttpStatusCode.OK, (await admin.PostJsonAsync("/api/v1/answerTemplate/create", new { Name = name, Content = "<p>x</p>" })).Status);
        int id = await FindTemplateIdAsync(admin, name);
        var template = await admin.GetAsync($"/api/v1/answerTemplate/getById/{id}");
        template.Json!["Content"] = "<p>y</p>";
        var update = await admin.PostJsonAsync("/api/v1/answerTemplate/update", template.Json);

        Assert.Equal(HttpStatusCode.OK, update.Status);
        Assert.Equal("<p>y</p>", (string)(await admin.GetAsync($"/api/v1/answerTemplate/getById/{id}"))["Content"]);
    }

    [Fact]
    public async Task Saving_a_template_someone_else_changed_meanwhile_is_rejected()
    {
        var admin = await AdminAsync();
        string name = TestData.Unique("Şablon");
        await admin.PostJsonAsync("/api/v1/answerTemplate/create", new { Name = name, Content = "<p>x</p>" });
        int id = await FindTemplateIdAsync(admin, name);
        var mine = (await admin.GetAsync($"/api/v1/answerTemplate/getById/{id}")).Json!;
        var theirs = mine.DeepClone();

        theirs["Content"] = "<p>theirs</p>";
        Assert.Equal(HttpStatusCode.OK, (await admin.PostJsonAsync("/api/v1/answerTemplate/update", theirs)).Status);
        mine["Content"] = "<p>mine</p>";
        var stale = await admin.PostJsonAsync("/api/v1/answerTemplate/update", mine);

        Assert.NotEqual(HttpStatusCode.OK, stale.Status);
        Assert.Equal("<p>theirs</p>", (string)(await admin.GetAsync($"/api/v1/answerTemplate/getById/{id}"))["Content"]);
    }
}

public class UserManagementTests(TestEnvironment env)
{
    private Task<ApiClient> AdminAsync() => env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);

    private static async Task<string> FindUserIdAsync(ApiClient admin, string userName)
    {
        for (int page = 1; ; page++)
        {
            var list = await admin.GetAsync($"/api/v1/auth/index/{page}");
            if (!list["data"]!.Any()) return null;
            var match = list["data"]!.FirstOrDefault(u => (string)u["UserName"] == userName);
            if (match != null) return (string)match["UserId"];
        }
    }

    [Fact]
    public async Task User_list_shows_role_names()
    {
        var list = await (await AdminAsync()).GetAsync("/api/v1/auth/index/1");

        Assert.True((int)list["paging"]!["TotalItems"] >= 2);
        Assert.Contains(list["data"]!, u => (string)u["UserName"] == TestEnvironment.AdminUser && u["RoleNames"]!.Select(r => (string)r).SequenceEqual(["admin"]));
    }

    [Fact]
    public async Task Admin_can_create_a_user_assign_a_role_and_reset_the_password()
    {
        var admin = await AdminAsync();
        string userName = TestData.Unique("personel");

        var create = await admin.PostJsonAsync("/api/v1/auth/create", new { UserName = userName, Password = "abcdef", FriendlyName = "Personel" });
        await admin.PostJsonAsync("/api/v1/auth/updateUserRoles", new { UserName = userName, FriendlyName = "Personel 2", UserRoles = new[] { "editor" } });
        await admin.PostJsonAsync("/api/v1/auth/changePassword", new { UserName = userName, Password = "yenisi" });

        Assert.Equal(HttpStatusCode.OK, create.Status);
        Assert.Equal("Üyelik bilgileriniz alınmıştır.", (string)create.Json);   // a JSON string, as Web API 2 returned it
        var roles = await admin.GetAsync($"/api/v1/auth/getUserRoles/{await FindUserIdAsync(admin, userName)}");
        Assert.Equal(["editor"], roles["UserRoles"]!.Select(r => (string)r));
        Assert.Equal("Personel 2", (string)roles["FriendlyName"]);
        var editor = await env.CreateClient().LoginAsync(userName, "yenisi");
        Assert.Equal(HttpStatusCode.OK, (await editor.GetAsync("/api/v1/dismissalcase/index/1")).Status);
    }

    [Fact]
    public async Task All_roles_are_listed()
    {
        var roles = await (await AdminAsync()).GetAsync("/api/v1/auth/getAllRoles");

        Assert.Superset(new HashSet<string> { "admin", "editor", "user" }, roles.Json!.Select(r => (string)r).ToHashSet());
    }
}

public class ErrorHandlingTests(TestEnvironment env)
{
    [Fact]
    public async Task Unexpected_errors_return_500_and_email_the_error_recipient()
    {
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);
        int before = await ErrorEmailsForAsync("MakeRead");

        var response = await admin.PostJsonAsync("/api/v1/dismissalcase/makeRead", new { Id = 999999 });

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(before + 1, await ErrorEmailsForAsync("MakeRead"));
    }

    // Other tests run in parallel and may trigger error emails too: count only this action's.
    private async Task<int> ErrorEmailsForAsync(string action)
    {
        int count = 0;
        foreach (var message in await env.Mail.MessagesToAsync("errors@tests.local"))
            if ((await env.Mail.HtmlAsync(message)).Contains($"ActionName: {action} ")) count++;
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
        string snapshot = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "db/tests/schema-snapshot.sql"));

        var legacy = await SqlScript.QueryLinesAsync(env.LegacyConnectionString, snapshot);
        var fresh = await SqlScript.QueryLinesAsync(env.FreshConnectionString, snapshot);

        Assert.Equal(["COL AspNetUsers.LockoutEndDateUtc datetime NULL"], legacy.Except(fresh));
        Assert.Empty(fresh.Except(legacy));
    }
}
