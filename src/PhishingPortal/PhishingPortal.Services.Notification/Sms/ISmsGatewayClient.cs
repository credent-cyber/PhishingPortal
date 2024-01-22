namespace PhishingPortal.Services.Notification.Sms
{
    public interface ISmsGatewayClient
    {
        Task<(bool, string)> Send(string to, string from, string message, string TemplateId);
        Task<decimal> GetBalance();
    }
}