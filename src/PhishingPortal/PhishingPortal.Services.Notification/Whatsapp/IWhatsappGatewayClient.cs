namespace PhishingPortal.Services.Notification.Whatsapp
{
    public interface IWhatsappGatewayClient
    {
        Task<bool> Send(string to, string from, string message);

        Task<bool> Send(string to, string from, string message, string mediaUrl, string file);
    }
}