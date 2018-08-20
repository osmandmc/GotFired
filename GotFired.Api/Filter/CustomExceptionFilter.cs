using GotFired.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace GotFired.Api.Filter
{
    public class CustomExceptionFilter: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            SmtpManager smtp = new SmtpManager();
            var exceptionMail = String.Format("ActionName: {0} Exception Message: {1} InnerException Message: {2} ", actionExecutedContext.ActionContext.ActionDescriptor.ActionName, actionExecutedContext.Exception.Message, actionExecutedContext.Exception.InnerException.Message);
            smtp.ExceptionMail(exceptionMail);
            base.OnException(actionExecutedContext);
        }
    }
}