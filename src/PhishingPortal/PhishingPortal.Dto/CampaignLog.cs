using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        public virtual Campaign Camp { get; set; }
        public virtual Recipient Recipient { get; set; }

        // public virtual CampaignTemplate Template { get; set; }
        //public virtual CampaignDetail Details { get; set; }

        /// <summary>
        /// This will record if the campaign email is forwarded to a designated mailbox from the recipient
        /// </summary>
        public bool IsReported { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportedOn { get; set; }
        public string Status { get; set; }
        public string SecurityStamp { get; set; }
    }

}
