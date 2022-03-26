using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class EmailTemplate : Auditable
    {
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }
}
