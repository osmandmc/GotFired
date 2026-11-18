namespace GotFired.Business
{
    /// <summary>Bound from the "Smtp" configuration section (replaces Web.config system.net/mailSettings).</summary>
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; }
        public int Port { get; set; } = 587;
        /// <summary>Use STARTTLS (Web.config had enableSsl="true" on port 587).</summary>
        public bool UseStartTls { get; set; } = true;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        /// <summary>Recipient of unhandled-exception emails. Empty disables them.</summary>
        public string ExceptionRecipient { get; set; }
        /// <summary>Public base URL used in the applicant link, e.g. https://istenatildimdestek.tk</summary>
        public string PublicBaseUrl { get; set; }
    }
}
