using PhishingPortal.Dto.Subscription;
using System.ComponentModel;
using Standard.Licensing;
using System.Runtime.Serialization;
using Standard.Licensing.Validation;
using Microsoft.Extensions.Logging;
using PhishingPortal.Dto;

namespace PhishingPortal.Licensing
{
    public class LicenseProvider : ILicenseProvider
    {
        private const string AllowedUserCountFeature = "AllowedUserCount";

        public ILogger<LicenseProvider> Logger { get; }

        public LicenseProvider(ILogger<LicenseProvider> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="passPhrase"></param>
        /// <param name="subscriptionInfo"></param>
        /// <returns></returns>
        public LicenseInfo Generate(string passPhrase, SubscriptionInfo subscriptionInfo)
        {
            var keyGenerator = Standard.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();
            var privateKey = keyPair.ToEncryptedPrivateKeyString(passPhrase);
            var publicKey = keyPair.ToPublicKeyString();

            var features = new Dictionary<string, string>();

            foreach (var info in subscriptionInfo.Modules)
            {
                features.Add(info.ToString(), subscriptionInfo.TransactionCount.ToString());
            }

            features.Add(AllowedUserCountFeature, subscriptionInfo.AllowedUserCount.ToString());
            features.Add(nameof(LicenseType), subscriptionInfo.SubscriptionType.ToString());

            var license = Standard.Licensing.License.New()
                            .WithUniqueIdentifier(Guid.NewGuid())
                            .As(LicenseType.Standard)
                            .ExpiresAt(subscriptionInfo.ExpiryInUTC)
                            .WithMaximumUtilization(subscriptionInfo.TransactionCount)
                            .WithProductFeatures(features)
                            .LicensedTo(subscriptionInfo.TenantIdentifier, subscriptionInfo.TenantEmail)
                            .CreateAndSignWithPrivateKey(privateKey, passPhrase);

            return new LicenseInfo(privateKey, publicKey, passPhrase, license.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="licString"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public SubscriptionInfo? GetSubscriptionInfo(string licString, string publicKey)
        {
            var result = Validate(licString, publicKey);

            return result.SubscriptionInfo;
        }

        public (bool Valid, SubscriptionInfo? SubscriptionInfo) Validate(string licString, string publicKey)
        {
            var content = System.Text.Encoding.UTF8.GetBytes(licString);

            using (MemoryStream ms = new MemoryStream(content))
            {
                var lic = Standard.Licensing.License.Load(ms);

                var validationFailures = lic.Validate().ExpirationDate(systemDateTime: DateTime.UtcNow)
                                .When(lic => lic.Type == LicenseType.Standard)
                                .And()
                                .Signature(publicKey)
                                .AssertValidLicense();

                var features = lic.ProductFeatures;
                var modules = new List<AppModules>();

                foreach (var module in features.GetAll())
                {
                    if (Enum.TryParse<AppModules>(module.Key, false, out AppModules appModule))
                    {
                        modules.Add(appModule);
                    }
                }

                var subscription = ExtractSubscriptioInfo(lic, features, modules);

                return (!validationFailures.Any(), subscription);

            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lic"></param>
        /// <param name="features"></param>
        /// <param name="modules"></param>
        /// <returns></returns>
        private SubscriptionInfo? ExtractSubscriptioInfo(Standard.Licensing.License lic, LicenseAttributes features, List<AppModules> modules)
        {
            try
            {
                return new SubscriptionInfo
                {
                    ExpiryInUTC = lic.Expiration,
                    SubscriptionType = Enum.Parse<LicenseTypes>(features.Get("LicenseType")),
                    AllowedUserCount = int.Parse(features.Get(AllowedUserCountFeature)),
                    TransactionCount = lic.Quantity,
                    Modules = modules,
                    TenantEmail = lic.Customer.Email,
                    TenantIdentifier = lic.Customer.Name
                };
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error parsing license");
                return default;
            }
        }
    }
}
