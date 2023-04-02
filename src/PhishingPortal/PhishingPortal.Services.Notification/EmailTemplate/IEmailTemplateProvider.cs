namespace PhishingPortal.Services.Notification.EmailTemplate
{
    public interface IEmailTemplateProvider
    {
        string GetEmailBody(string templateName, Dictionary<string, string> parameter);
    }
}