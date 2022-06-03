using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{

    public enum CampaignLogStatus
    {
        Unknown,
        Sent,
        Error,
        Expired,
        Completed
    }

    public class CampaignLog : Auditable
    {
        public int CampaignId { get; set; }
        public int RecipientId { get; set; }
        public string CampignType { get; set; }
        public DateTime SentOn { get; set; }
        public string SentBy { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsHit { get; set; }
        public bool IsDataEntered { get; set; }
        public bool IsAttachmentOpen { get; set; }
        public bool IsMacroEnabled { get; set; }
        public bool IsReplied { get; set; }
        public string Status { get; set; }
        public string SecurityStamp { get; set; }
    }

}
