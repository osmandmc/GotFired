using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Infrastructure
{
    /// <summary>
    /// A save that still conflicts after retrying (heavy concurrent editing of one case): 409 Conflict as Problem
    /// Details; the client can retry. Expected under load, so not emailed as an error.
    /// </summary>
    public class ConflictExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not DbUpdateConcurrencyException) return false;

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = { Status = StatusCodes.Status409Conflict, Title = "Kayıt aynı anda başka biri tarafından değiştirildi; lütfen tekrar deneyin." },
            });
        }
    }
}
