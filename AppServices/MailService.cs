using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using Task_2EF.Configuration;

namespace Task_2EF.AppServices
{
    public class MailService : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        public MailService(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_emailConfig.Display, _emailConfig.From);
            message.Subject = subject;
            message.From.Add(message.Sender);
            message.To.Add(MailboxAddress.Parse(email));

            message.Body = new BodyBuilder() { HtmlBody = htmlMessage }.ToMessageBody();
            /*
            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;
            message.Body = builder.ToMessageBody();*/

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    smtp.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_emailConfig.Username, _emailConfig.Password);
                    await smtp.SendAsync(message);
                }
                catch (Exception ex)
                {
                    System.IO.Directory.CreateDirectory("mailssave");
                    var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
                    await message.WriteToAsync(emailsavefile);
                    throw ex;
                }
                finally
                {
                    smtp.Disconnect(true);
                    smtp.Dispose();
                }
            }

        }
    }
}
