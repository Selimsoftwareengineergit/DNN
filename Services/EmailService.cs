using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DNN.Data;
using DNN.Models; // Your User model namespace

namespace DNN.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Method to send Reset Password email
        public async Task SendResetPasswordEmailAsync(User user, string newPassword)
        {
            var subject = "Your New Password";
            var body = $@"
                <p>Hello {user.FullName},</p>
                <p>Your new password is: <b>{newPassword}</b></p>
                <p>Regards,<br/>Hello World It Support</p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        // Method to send "Know Old Password" email (password not recoverable)
        public async Task SendPasswordNotRecoverableEmailAsync(User user)
        {
            var subject = "Password Recovery";
            var body = $@"
                <p>Hello {user.FullName},</p>
                <p>Your password is not recoverable for security reasons. Please use the Reset Password option instead.</p>
                <p>Regards,<br/>Hello World It Support</p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        // Generic private method to send email
        private async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var smtpHost = emailSettings.GetValue<string>("SmtpHost");
            var smtpPort = emailSettings.GetValue<int>("SmtpPort");
            var fromName = emailSettings.GetValue<string>("FromName");
            var fromEmail = emailSettings.GetValue<string>("FromEmail");
            var emailPassword = emailSettings.GetValue<string>("EmailPassword");

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(fromEmail, emailPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}