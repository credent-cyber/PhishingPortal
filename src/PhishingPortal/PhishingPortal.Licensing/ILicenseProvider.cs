using PhishingPortal.Dto.Subscription;

namespace PhishingPortal.Licensing
{
    public interface ILicenseProvider
    {
        LicenseInfo Generate(string passPhrase, SubscriptionInfo subscriptionInfo);
        SubscriptionInfo? GetSubscriptionInfo(string licString, string publicKey);
        (bool Valid, SubscriptionInfo? SubscriptionInfo) Validate(string licString, string publicKey);
    }
}