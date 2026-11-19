using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GotFired.Api.Tests.Infrastructure;

/// <summary>Sends requests the way the existing front end does and returns status, parsed JSON and headers.</summary>
public sealed class ApiClient(HttpClient http, Mailpit mail)
{
    public Mailpit Mail { get; } = mail;
    private string _token;

    /// <summary>Logs in like login.html: form text posted to /token WITHOUT a form content type (text/plain).</summary>
    public async Task<ApiResponse> RequestTokenAsync(string user, string password)
    {
        var content = new StringContent($"grant_type=password&username={user}&password={password}", Encoding.UTF8, "text/plain");
        return await ApiResponse.FromAsync(await http.PostAsync("/token", content));
    }

    public async Task<ApiClient> LoginAsync(string user, string password)
    {
        var response = await RequestTokenAsync(user, password);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        _token = (string)response.Json["access_token"];
        return this;
    }

    public Task<ApiResponse> GetAsync(string path) => SendAsync(HttpMethod.Get, path, null);

    /// <summary>JSON body, as the AngularJS $resource calls send it.</summary>
    public Task<ApiResponse> PostJsonAsync(string path, object body) =>
        SendAsync(HttpMethod.Post, path, new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

    /// <summary>Form body, as apply.html and userView.html post with jQuery .serialize().</summary>
    public Task<ApiResponse> PostFormAsync(string path, IEnumerable<KeyValuePair<string, string>> fields) =>
        SendAsync(HttpMethod.Post, path, new FormUrlEncodedContent(fields));

    private async Task<ApiResponse> SendAsync(HttpMethod method, string path, HttpContent content)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        if (_token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return await ApiResponse.FromAsync(await http.SendAsync(request));
    }
}

public sealed record ApiResponse(HttpStatusCode Status, JToken Json, string Body, HttpResponseHeaders Headers, HttpContentHeaders ContentHeaders)
{
    public static async Task<ApiResponse> FromAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        JToken json = null;
        try { if (body.Length > 0) json = JToken.Parse(body); } catch (Newtonsoft.Json.JsonReaderException) { }
        return new ApiResponse(response.StatusCode, json, body, response.Headers, response.Content.Headers);
    }

    public JToken this[string key] => Json?[key];
}
