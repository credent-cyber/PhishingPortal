using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class EmailTemplate
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }
}
