using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class EmailCampaign
    {
        public int Id { get; set; }
        public int EmailTemplateId { get; set; }
        public string UrlLink { get; set; }
    }
}
