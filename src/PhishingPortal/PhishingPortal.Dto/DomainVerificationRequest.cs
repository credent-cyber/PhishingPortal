using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto
{
    public class DomainVerificationRequest
    {
        public string UniqueId { get; set; }

        public string Domain { get; set; }

        public string DomainVerificationCode { get; set; }
        
    }


    public class TenantDomain : Auditable
    {
        public int TenantId { get; set; }

        public string Domain { get; set; }

        public string DomainVerificationCode { get; set; }

        public bool IsDomainVerified { get; set; }

    }

}
