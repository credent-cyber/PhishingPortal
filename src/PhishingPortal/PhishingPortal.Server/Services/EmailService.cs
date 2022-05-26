using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace PhishingPortal.Server.Services;

public class SmtpMessageSenderOptions
{
    public string Server { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string FromEmail { get; set; } = "malaykp.devices@gmail.com";
    public string Password { get; set; } = "***********";

}

public class SendGridMessagerSenderOptions
{
    public string? SendGridKey { get; set; }
}

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public EmailSender(IOptions<SmtpMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    public SmtpMessageSenderOptions Options { get; } //Set with Secret Manager.

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        //if (string.IsNullOrEmpty(Options.SendGridKey))
        //{
        //    throw new Exception("Null SendGridKey");
        //}
        await Execute(string.Empty, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {


        _logger.LogInformation("Sending email");

        var client = new SmtpClient(Options.Server, Options.Port)
        {
            Credentials = new NetworkCredential(Options.FromEmail, Options.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage();
        mailMessage.IsBodyHtml = true;
        mailMessage.Body = message;
        mailMessage.Subject = subject;
        mailMessage.From = new MailAddress(Options.FromEmail);
        mailMessage.To.Add(new MailAddress(toEmail));
        mailMessage.Sender = new MailAddress(Options.FromEmail);

        await client.SendMailAsync(mailMessage);

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
