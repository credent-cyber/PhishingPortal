using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Recipient : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Mobile { get; set; }
        public int WhatsAppNo { get; set; }
    }
}
