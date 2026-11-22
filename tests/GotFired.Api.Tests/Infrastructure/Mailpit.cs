using Newtonsoft.Json.Linq;

namespace GotFired.Api.Tests.Infrastructure;

/// <summary>Reads email captured by the Mailpit container.</summary>
public sealed class Mailpit(Uri baseAddress)
{
    private readonly HttpClient _http = new() { BaseAddress = baseAddress };

    public async Task<List<JToken>> MessagesToAsync(string address)
    {
        var all = JObject.Parse(await _http.GetStringAsync("/api/v1/messages?limit=500"))["messages"]!;
        return all.Where(m => m["To"]!.Any(t => (string)t["Address"] == address)).ToList();   // newest first
    }

    public async Task<List<JToken>> MessagesWithSubjectAsync(string prefix)
    {
        var all = JObject.Parse(await _http.GetStringAsync("/api/v1/messages?limit=500"))["messages"]!;
        return all.Where(m => ((string)m["Subject"]).StartsWith(prefix, StringComparison.Ordinal)).ToList();
    }

    public async Task<string> HtmlAsync(JToken message) =>
        (string)JObject.Parse(await _http.GetStringAsync($"/api/v1/message/{message["ID"]}"))["HTML"];

    /// <summary>The applicant link token from the newest email to <paramref name="address"/>.</summary>
    public async Task<string> LatestApplicantLinkTokenAsync(string address)
    {
        var messages = await MessagesToAsync(address);
        if (messages.Count == 0) return null;
        string html = await HtmlAsync(messages[0]);
        const string marker = "userView.html#token=";
        int start = html.IndexOf(marker, StringComparison.Ordinal);
        return start < 0 ? null : html[(start + marker.Length)..].Split('\'')[0];
    }
}
