namespace PhishingPortal.Services.Notification.Sms
{
    public interface ISmsGatewayClient
    {
        Task<bool> Send(string to, string from, string message);
        Task<decimal> GetBalance();
    }
}