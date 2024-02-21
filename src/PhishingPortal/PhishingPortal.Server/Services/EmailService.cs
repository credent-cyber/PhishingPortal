using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhishingPortal.Common;
using System.Net;
using System.Net.Mail;

namespace PhishingPortal.Server.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly IEmailClient _emailClient;
    
    public EmailSender(ILogger<EmailSender> logger, IEmailClient emailClient)
    {
        _logger = logger;
        _emailClient = emailClient;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        //await Execute(email, subject, htmlMessage);

        await _emailClient.sendEmailAsync(email, subject, htmlMessage, true, string.Empty, string.Empty);
    }

    public async Task Execute(string email, string subject, string message)
    {
        _logger.LogInformation("Sending email");
        
        var mailMessage = new MailMessage();
        mailMessage.IsBodyHtml = true;
        mailMessage.Body = message;
        mailMessage.Subject = subject;
        mailMessage.To.Add(new MailAddress(email));

        if(_emailClient == null)
        {
            _logger.LogError($"Email client not initialized");
            return;
        }
        
            
        await _emailClient.SendEmailAsync(mailMessage);
        
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
