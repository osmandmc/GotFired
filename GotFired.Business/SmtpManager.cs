using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GotFired.Business
{
    public class SmtpManager
    {
        private readonly SmtpOptions _options;

        public SmtpManager(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public void SendEmail(string emailAdress, string guid)
        {
            string link = $"{_options.PublicBaseUrl?.TrimEnd('/')}/userview.html?guid={Uri.EscapeDataString(guid ?? string.Empty)}";
            Send(emailAdress,
                "İşten Atıldım Başvurunuz Hk.",
                "Merhaba,  <p>istenatildim.org sitesine yapmış olduğunuz başvurunun cevabını görmek ve isterseniz görüşmeyi devam ettirmek için şu <a href = '" + link + "'>linke</a> tıklayın.</p>");
        }

        public void ExceptionMail(string text)
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
