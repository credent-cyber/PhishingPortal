using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class RecipientGroup : BaseEntity
    {
        public string Uid { get; set; }
        public string GroupName { get; set; }
        public bool IsActiveDirectoryGroup { get; set; } = false;
        public DateTime LastImported { get; set; }

    }
}
