using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Recipient : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string WhatsAppNo { get; set; }
    }

    public class RecipientImport
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string ValidationErrMsg { get; set; }

    }


    public enum RecipientSources
    {
        None = 0,
        Csv = 1,
       // Xls,
        ActiveDirectory,
       // RecipientGroup
    }
}
 