using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Business
{
    public class SmtpManager
    {
        private readonly SmtpClient _smtpClient;
        public SmtpManager()
        {
            _smtpClient = new SmtpClient();
        }
        public void SendEmail(string emailAdress, string guid)
        {
            //SmtpClient smtpClient = new SmtpClient();
            MailMessage mailMessage = new MailMessage();

            mailMessage.To.Add(new MailAddress(emailAdress));
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "İşten Atıldım Başvurunuz Hk.";
            mailMessage.Body = "Merhaba,  <p>istenatildim.org sitesine yapmış olduğunuz başvurunun cevabını görmek ve isterseniz görüşmeyi devam ettirmek için şu <a href = 'http://istenatildimdestek.tk/userview.html?guid=" + guid + "'>linke</a> tıklayın.</p>";
            _smtpClient.Send(mailMessage);
        }
        public void ExceptionMail(string text)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.To.Add(new MailAddress("osmandmc@gmail.com"));
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "Exception: istenatildimdestek.tk";
            mailMessage.Body = text;
            _smtpClient.Send(mailMessage);
        }
      
    }
}
