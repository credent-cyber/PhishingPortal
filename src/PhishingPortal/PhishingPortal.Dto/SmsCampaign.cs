using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class SmsCampaign : BaseEntity
    {
        public int SmsTemplateId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
