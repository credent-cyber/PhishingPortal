using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class WhatsappCampaign
    {
        public int Id { get; set; }
        public int WhatsAppTemplateId { get; set; }
        public string Link { get; set; }

    }
}
