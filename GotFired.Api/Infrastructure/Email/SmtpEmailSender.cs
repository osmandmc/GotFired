using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GotFired.Api.Infrastructure.Email
{
    public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
    {
        private readonly SmtpOptions _options = options.Value;

        public void SendApplicantLink(string emailAddress, string accessToken)
        {
            // The token goes in the URL fragment: browsers never send fragments to the server, so it stays out of
            // access logs and Referer headers. The file name is case-sensitive on Linux.
            string link = $"{_options.PublicBaseUrl?.TrimEnd('/')}/userView.html#token={Uri.EscapeDataString(accessToken ?? string.Empty)}";
            Send(emailAddress,
                "İşten Atıldım Başvurunuz Hk.",
                "Merhaba,  <p>istenatildim.org sitesine yapmış olduğunuz başvurunun cevabını görmek ve isterseniz görüşmeyi devam ettirmek için şu <a href = '" + link + "'>linke</a> tıklayın.</p>");
        }

        public void SendErrorReport(string text)
        {
            if (string.IsNullOrWhiteSpace(_options.ExceptionRecipient)) return;
            Send(_options.ExceptionRecipient, "Exception: " + _options.PublicBaseUrl, text);
        }

        private void Send(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_options.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            client.Connect(_options.Host, _options.Port, _options.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            if (!string.IsNullOrEmpty(_options.UserName))
            {
                client.Authenticate(_options.UserName, _options.Password);
            }
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
