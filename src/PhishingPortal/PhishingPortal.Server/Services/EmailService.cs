using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace PhishingPortal.Server.Services
{

    public class AuthMessageSenderOptions
    {
        //public string? SendGridKey { get; set; }
    }

    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                           ILogger<EmailSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            //if (string.IsNullOrEmpty(Options.SendGridKey))
            //{
            //    throw new Exception("Null SendGridKey");
            //}
            await Execute(String.Empty, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {


            _logger.LogInformation("Sending email");

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("malaykp.devices@gmail.com", "**********"),
                EnableSsl = true
            };
            
            await client.SendMailAsync("malaykp.devices@gmail.com", toEmail, subject, message);

            _logger.LogInformation("Mail sent");

            //var client = new SendGridClient(apiKey);
            //var msg = new SendGridMessage()
            //{
            //    From = new EmailAddress("Joe@contoso.com", "Password Recovery"),
            //    Subject = subject,
            //    PlainTextContent = message,
            //    HtmlContent = message
            //};
            //msg.AddTo(new EmailAddress(toEmail));

            //// Disable click tracking.
            //// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            //msg.SetClickTracking(false, false);
            //var response = await client.SendEmailAsync(msg);
            //_logger.LogInformation(response.IsSuccessStatusCode
            //                       ? $"Email to {toEmail} queued successfully!"
            //                       : $"Failure Email to {toEmail}");
        }
    }
}
