using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class EmailCampaign : BaseEntity
    {
        public int EmailTemplateId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
