using GotFired.Api.Infrastructure.Email;
using Microsoft.AspNetCore.Diagnostics;

namespace GotFired.Api.Infrastructure
{
    /// <summary>
    /// Replaces CustomExceptionFilter: logs the exception, emails it to Smtp:ExceptionRecipient,
    /// then lets the default handler return a 500 (the SPA shows a generic toast on 500).
    /// </summary>
    public class EmailExceptionHandler : IExceptionHandler
    {
        private readonly IEmailSender _email;
        private readonly ILogger<EmailExceptionHandler> _logger;

        public EmailExceptionHandler(IEmailSender email, ILogger<EmailExceptionHandler> logger)
        {
            _email = email;
            _logger = logger;
        }

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // The exception middleware clears the current endpoint before handlers run; the feature keeps the original.
            // DisplayName is the route, e.g. "HTTP: PUT /api/v1/cases/{id:int}/appeal-state".
            var endpoint = httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint ?? httpContext.GetEndpoint();
            string action = endpoint?.DisplayName ?? httpContext.Request.Path;
            _logger.LogError(exception, "Unhandled exception in {Action}", action);

            try
            {
                _email.SendErrorReport(string.Format("ActionName: {0} Exception Message: {1} InnerException Message: {2} ",
                    action, exception.Message, exception.InnerException?.Message));
            }
            catch (Exception mailException)
            {
                _logger.LogError(mailException, "Could not send exception email");
            }

            // Not handled here: fall through to the default ProblemDetails 500 response.
            return ValueTask.FromResult(false);
        }
    }
}
