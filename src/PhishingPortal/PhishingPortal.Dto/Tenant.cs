using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
        public string UrlPrefix { get; set; }

        [Required]
        [RegularExpression("^(?!-)[A-Za-z0-9-]+([\\-\\.]{1}[a-z0-9]+)*\\.[A-Za-z]{2,6}$", ErrorMessage = "Invalid Domain")]
        public string Domain { get; set; }
        public string DomainVerificationCode { get; set; }
        public string IsDomainVerified { get; set; }
        public DbOptions DatabaseOption { get; set; } = DbOptions.SqlLite; // Sqlite || MySql || Postgre || MS Sql Server
        public virtual ICollection<TenantData> SettingsData { get; set; }

    }

    public enum DbOptions
    {
        SqlLite,
        MsSql,
        PostGreSql
    }

    public class ProvisionTenantRequest
    {
        public int TenantId { get; set; }
        public string ConnectionString { get; set; }
    }

}
