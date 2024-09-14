using PhishingPortal.Dto.Subscription;

namespace PhishingPortal.Licensing
{
    public interface ILicenseProvider
    {
        (string PrivateKey, string PublicKey, string PassPhrase, string Content) Generate(string passPhrase, SubscriptionInfo subscriptionInfo);
        SubscriptionInfo? GetSubscriptionInfo(string content, string publicKey);
        (bool Valid, SubscriptionInfo? SubscriptionInfo) Validate(string content, string publicKey);
    }
}