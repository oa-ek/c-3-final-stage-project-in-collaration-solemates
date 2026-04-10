using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace StepStyle.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = _configuration["EmailSettings:Sender"];
            var pw = _configuration["EmailSettings:Password"];

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };

            var message = new MailMessage(
                from: mail,
                to: email,
                subject,
                htmlMessage)
            {
                IsBodyHtml = true 
            };

            return client.SendMailAsync(message);
        }
    }
}