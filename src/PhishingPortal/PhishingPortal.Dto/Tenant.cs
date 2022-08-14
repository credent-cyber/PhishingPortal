using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using PhishingPortal.Common;

namespace PhishingPortal.Dto
{

    public class Tenant : Auditable
    {
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string ContactEmail { get; set; }
        public string ContactNumber { get; set; }
        public string UniqueId { get; set; }
        public string ConfirmationLink { get; set; }
        public ConfirmationStats ConfirmationState { get; set; } = ConfirmationStats.Registered;
        public DateTime ConfirmationExpiry { get; set; }
        public DbOptions DatabaseOption { get; set; } = DbOptions.SqlLite; // Sqlite || MySql || Postgre || MS Sql Server
        public LicenseTypes LicenseType { get; set; } = LicenseTypes.Subscription;
        public DateTime LicenseExpiry { get; set; }
        public string LicenseKey { get; set; }
        public bool IsActive { get; set; }
        public bool RequireDomainVerification { get; set; }
        public virtual ICollection<TenantData> Settings { get; set; }
        public virtual ICollection<TenantDomain> TenantDomains { get; set; }

        public override string ToString()
        {
            return $"{UniqueId}|{ConfirmationState}|{LicenseType}|{DatabaseOption}";
        }

        public string GetConfirmationLink(string baseUrl)
        {
            return $"{baseUrl}/{UniqueId}/{ToString().ComputeMd5Hash()}";
        }

    }

    public enum DbOptions
    {
        SqlLite,
        MySql,
        MsSql,
        PostGreSql
        
    }

    public enum LicenseTypes
    {
        Subscription,
        Users,
        Feature
    }

    public enum ConfirmationStats
    {
        Registered,
        Verified,
        DomainVerified,
        MasterUser,  // this the final state currently
        Licensed
    }

}
