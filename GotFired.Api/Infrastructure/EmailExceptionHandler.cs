using GotFired.Business;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GotFired.Api.Infrastructure
{
    /// <summary>
    /// Replaces CustomExceptionFilter: logs the exception, emails it to Smtp:ExceptionRecipient,
    /// then lets the default handler return a 500 (the SPA shows a generic toast on 500).
    /// </summary>
    public class EmailExceptionHandler : IExceptionHandler
    {
        private readonly SmtpManager _smtpManager;
        private readonly ILogger<EmailExceptionHandler> _logger;

        public EmailExceptionHandler(SmtpManager smtpManager, ILogger<EmailExceptionHandler> logger)
        {
            _smtpManager = smtpManager;
            _logger = logger;
        }

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            string action = httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ActionName
                            ?? httpContext.Request.Path;
            _logger.LogError(exception, "Unhandled exception in {Action}", action);

            try
            {
                _smtpManager.ExceptionMail(string.Format("ActionName: {0} Exception Message: {1} InnerException Message: {2} ",
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
