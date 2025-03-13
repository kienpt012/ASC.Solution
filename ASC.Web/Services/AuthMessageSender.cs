using ASC.Web.Configuration;
using MailKit.Net.Smtp;
using ASC.Web.Services;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
namespace ASC.Web.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender // Hoàn thành khai báo ISmsSender
    {
        private readonly IOptions<ApplicationSettings> _settings;

        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("admin", _settings.Value.SMTPAccount));
            emailMessage.To.Add(new MailboxAddress("user", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new MailKit.Net.Smtp.SmtpClient()) // Chỉ định rõ MailKit.SmtpClient
            {
                await client.ConnectAsync(_settings.Value.SMTPServer, _settings.Value.SMTPPort, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_settings.Value.SMTPAccount, _settings.Value.SMTPPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        // Triển khai phương thức từ ISmsSender (nếu cần)
        public Task SendSmsAsync(string number, string message)
        {
            throw new NotImplementedException("SMS sending is not implemented.");
        }
    }
}