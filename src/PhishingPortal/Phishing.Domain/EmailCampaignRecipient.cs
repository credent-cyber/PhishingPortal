﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class EmailCampaignRecipient
    {
        public int CampaignId { get; set; }
        public int EmployeeId { get; set; }
    }
}
