using GotFired.Model;
using Microsoft.AspNetCore.Diagnostics;

namespace GotFired.Api.Infrastructure
{
    /// <summary>
    /// Rule violations become HTTP 412 { StatusMessage, StatusText }: the SPA's interceptor (app/app.js) shows them as a
    /// warning toast. They are expected outcomes, so they are neither logged as errors nor emailed.
    /// </summary>
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException) return false;

            httpContext.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
            await httpContext.Response.WriteAsJsonAsync(new Dictionary<string, string>
            {
                ["StatusMessage"] = exception.Message,
                ["StatusText"] = "Uyarı",
            }, cancellationToken);
            return true;
        }
    }
}
