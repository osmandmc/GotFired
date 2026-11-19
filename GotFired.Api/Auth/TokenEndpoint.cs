using GotFired.Business.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace GotFired.Api.Auth
{
    /// <summary>
    /// Drop-in replacement for the OWIN OAuth "resource owner password" endpoint at /token.
    /// login.html posts "grant_type=password&username=..&password=.." WITHOUT a form content type
    /// (XMLHttpRequest sends text/plain), so the body is parsed as a query string regardless of content type.
    /// The response shape (access_token, token_type, expires_in, userName) is what app/service.js reads.
    /// </summary>
    public static class TokenEndpoint
    {
        public static IEndpointRouteBuilder MapTokenEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/token", HandleAsync).AllowAnonymous();
            return app;
        }

        private static async Task<IResult> HandleAsync(HttpRequest request, IUserService users, TokenService tokenService)
        {
            using var reader = new StreamReader(request.Body);
            var form = QueryHelpers.ParseQuery(await reader.ReadToEndAsync());

            if (form["grant_type"] != "password")
            {
                return Results.BadRequest(new { error = "unsupported_grant_type" });
            }

            var user = await users.FindUserAsync(form["username"], form["password"]);
            if (user == null)
            {
                return Results.BadRequest(new { error = "invalid_grant", error_description = "The user name or password is incorrect." });
            }

            // Property names are written exactly as the OWIN server returned them.
            return Results.Json(new Dictionary<string, object>
            {
                ["access_token"] = tokenService.CreateToken(user),
                ["token_type"] = "bearer",
                ["expires_in"] = (int)tokenService.Lifetime.TotalSeconds,
                ["userName"] = user.UserName,
            });
        }
    }
}
