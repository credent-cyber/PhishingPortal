using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class WhatsappCampaign : BaseEntity
    {
        public int WhatsAppTemplateId { get; set; }
        public string ReturnUrl { get; set; }

    }
}
