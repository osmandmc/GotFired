namespace GotFired.Api.Infrastructure.Email
{
    public interface IEmailSender
    {
        /// <summary>Tells the applicant there is an answer, with their secret link to userView.html.</summary>
        void SendApplicantLink(string emailAddress, string accessToken);

        /// <summary>Reports an unhandled error to Smtp:ExceptionRecipient (no-op when not configured).</summary>
        void SendErrorReport(string text);
    }
}
