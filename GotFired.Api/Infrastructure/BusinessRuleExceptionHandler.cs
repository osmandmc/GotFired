using GotFired.Model;
using Microsoft.AspNetCore.Diagnostics;

namespace GotFired.Api.Infrastructure
{
    /// <summary>
    /// A request the business rules reject: 422 Unprocessable Content as Problem Details, with the reason in
    /// "detail" (shown by the SPA as a warning toast). Expected outcomes: not logged as errors, not emailed.
    /// </summary>
    public class BusinessRuleExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException) return false;

            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = { Status = StatusCodes.Status422UnprocessableEntity, Title = "İşlem yapılamadı.", Detail = exception.Message },
            });
        }
    }
}
