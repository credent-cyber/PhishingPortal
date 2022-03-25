using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class SmsCampaign
    {
        public int Id { get; set; }
        public int SmsTemplateId { get; set; }
        public string Link { get; set; }
    }
}
