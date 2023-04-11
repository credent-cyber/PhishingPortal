namespace PhishingPortal.Services.Notification.EmailTemplate
{
    public class EmailTemplateProvider : IEmailTemplateProvider
    {
        private string templatePath_ = "./EmailTemplates";

        public EmailTemplateProvider(ILogger<EmailTemplateProvider> logger, IConfiguration appsettings)
        {
            templatePath_ = appsettings.GetValue<string>("EmailTemplatePath") ?? "./EmailTemplates";
            Logger = logger;
        }

        public ILogger<EmailTemplateProvider> Logger { get; }

        public string GetEmailBody(string templateName, Dictionary<string, string> parameter)
        {

            try
            {
                if (Directory.Exists(templatePath_))
                    throw new Exception("Template Path Does not exists");

                var templateFilePath = Path.Combine(templatePath_, $"{templateName}.htm");

                if (!File.Exists(templateFilePath))
                    throw new Exception("Template File doesn't exist");

                var content = File.ReadAllText(Path.Combine(templatePath_, $"{templateName}.htm"));

                foreach (var item in parameter)
                {
                    content = content.Replace(item.Key, item.Value);
                }

                return content;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error processing template file {templateName}");
                throw;
            }

        }
    }
}
