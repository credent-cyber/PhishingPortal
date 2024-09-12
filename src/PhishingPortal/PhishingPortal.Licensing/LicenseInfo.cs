namespace PhishingPortal.Licensing
{
    public class LicenseInfo
    {
        public string PrivateKey { get; protected set; } = string.Empty;
        public string PublicKey { get; protected set; } = string.Empty;
        public string PassPhrase { get; protected set; } = string.Empty;
        public string Content { get; protected set; } = string.Empty;

        public LicenseInfo(string privateKey, string publicKey, string passPhrase, string content)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
            PassPhrase = passPhrase;
            Content = content;
        }
    }
}
