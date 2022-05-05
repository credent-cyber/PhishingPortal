using System;
using System.Collections.Generic;
using System.Text;

namespace PhishingPortal.Dto.Admin
{
    public class Tenant : Auditable
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string ContactEmail { get; set; }
        public string ContactNumber { get; set; }
        public string UniqueId { get; set; }
        public string UrlPrefix { get; set; }
        public virtual ICollection<TenantData> SettingsData { get; set; }

    }
}
