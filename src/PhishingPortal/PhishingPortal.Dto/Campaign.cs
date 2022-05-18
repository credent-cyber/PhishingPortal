using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{

    public class Campaign : Auditable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CampaignStateEnum State { get; set; }
        public bool IsActive { get; set; }
        public virtual CampaignDetail Detail { get; set; }
        public virtual CampaingSchedule Schedule { get; set; }

    }
}
