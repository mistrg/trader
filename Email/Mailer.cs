using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace Trader.Email
{
    public interface IMailer
    {
        Task<string> SendEmailAsync( string subject, string body);
    }

    public class Mailer : IMailer
    {
        private readonly SmtpSettings _smtpSettings;

        public Mailer(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task<string> SendEmailAsync(string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));

                foreach (var email in _smtpSettings.To.Split(","))
                    message.To.Add(new MailboxAddress(email,email));
                message.Subject = subject;
                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;


                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, true);


                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                return $"SendEmail failed {e.Message} Message {body} not delivered";
            }
            return string.Empty;
        }
    }
}