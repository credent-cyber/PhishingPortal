using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Bcpg.OpenPgp;
using PhishingPortal.Common;

namespace PhishingPortal.Licensing
{
    //public class LicenseInfo
    //{
    //    public string PrivateKey { get; protected set; } = string.Empty;
    //    public string PublicKey { get; protected set; } = string.Empty;
    //    public string PassPhrase { get; protected set; } = string.Empty;
    //    public string Content { get; protected set; } = string.Empty;

    //    public LicenseInfo() { }

    //    public LicenseInfo(string privateKey, string publicKey, string passPhrase, string content)
    //    {
    //        PrivateKey = privateKey;
    //        PublicKey = publicKey;
    //        PassPhrase = passPhrase;
    //        Content = content;
    //    }

    //    public override string ToString()
    //    {
    //        return JsonConvert.SerializeObject(this).ToBase64String();
    //    }

    //    public string ToJson()
    //    {
    //        return JsonConvert.SerializeObject(this);
    //    }

    //    public LicenseInfo ToSubscriberCopy()
    //    {
    //        return new LicenseInfo(string.Empty, PublicKey, string.Empty, Content);
    //    }
    //}
}
