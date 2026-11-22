using System.Net;

namespace GotFired.Api.Tests.Infrastructure;

/// <summary>Creates the data a test needs through the API, with unique names so tests stay independent.</summary>
public static class TestData
{
    public static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..(prefix.Length + 9)];

    public static Dictionary<string, string> ApplicantToken(string token) => new() { ["X-Applicant-Token"] = token };

    /// <summary>Submits the public application form like apply.html (jQuery form post) and returns its submission Guid.</summary>
    public static async Task<(string Guid, string ApplicantName, string Email)> SubmitApplicationAsync(ApiClient anonymous)
    {
        var form = await anonymous.GetAsync("/api/v1/application-form");
        Assert.Equal(HttpStatusCode.OK, form.Status);
        string guid = (string)form["guid"];
        string name = Unique("Başvuru");
        string email = $"{Unique("applicant")}@example.com";

        var response = await anonymous.PostFormAsync("/api/v1/applications", ApplicationForm(guid, name, email, (int)form["cities"]![0]!["ID"]));
        Assert.Equal(HttpStatusCode.Accepted, response.Status);
        return (guid, name, email);
    }

    public static List<KeyValuePair<string, string>> ApplicationForm(string guid, string name, string email, int cityId) =>
    [
        new("Guid", guid), new("ApplicantFullName", name), new("ApplicantEmail", email),
        new("ApplicantPhoneNumber", "(555) 555 55 55"), new("ApplicantGender", "2"), new("ApplicantAgeInterval", "2"),
        new("ApplicantEducationalState", ""), new("CityID", cityId.ToString()), new("EmploymentTerminationDate", "2026-10-01T00:00:00.000Z"),
        new("EmploymentDurationSince", "3"), new("EmployeeCount", "1"), new("DismissalState", "2"), new("WantShareHisExperience", "true"),
        new("DetailedExperience", "deneyim"), new("AppealState", "1"), new("CompanySectorName", "Yazılım"), new("SGKTerminationReasonID", ""),
    ];

    /// <summary>Finds a case in the staff list by applicant name (searches all pages).</summary>
    public static async Task<int> FindCaseIdAsync(ApiClient admin, string applicantName)
    {
        for (int page = 1; ; page++)
        {
            var list = await admin.GetAsync($"/api/v1/cases?page={page}");
            Assert.Equal(HttpStatusCode.OK, list.Status);
            var data = list["data"]!;
            if (!data.Any()) throw new InvalidOperationException($"Case for {applicantName} not found.");
            var match = data.FirstOrDefault(c => (string)c["ApplicantFullName"] == applicantName);
            if (match != null) return (int)match["Id"];
        }
    }

    /// <summary>A new case and its id: submitted anonymously, found as staff.</summary>
    public static async Task<(int CaseId, string Guid, string ApplicantName, string Email)> CreateCaseAsync(TestEnvironment env)
    {
        var (guid, name, email) = await SubmitApplicationAsync(env.CreateClient());
        var admin = await env.CreateClient().LoginAsync(TestEnvironment.AdminUser, TestEnvironment.LegacyPassword);
        return (await FindCaseIdAsync(admin, name), guid, name, email);
    }

    /// <summary>A staff answer, as the case page sends it.</summary>
    public static Task<ApiResponse> AnswerAsync(ApiClient admin, int caseId, string text) =>
        admin.PostJsonAsync($"/api/v1/cases/{caseId}/comments", new { Text = text });
}
