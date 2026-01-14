namespace WebApplication1.Services
{
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
    using WebApplication1.DTOs;

    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(to);

            using var smtp = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(
                    _settings.SenderEmail,
                    _settings.Password
                ),
                EnableSsl = _settings.EnableSsl
            };
            smtp.Port = 25;
            await smtp.SendMailAsync(message);

        }
    }

}
