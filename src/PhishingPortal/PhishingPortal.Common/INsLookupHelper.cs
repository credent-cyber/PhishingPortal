using Microsoft.Extensions.Logging;

namespace PhishingPortal.Common
{
    public interface INsLookupHelper
    {
        ILogger<NsLookupHelper> Logger { get; }

        bool VerifyDnsRecords(string type, string domain, string value);
    }
}