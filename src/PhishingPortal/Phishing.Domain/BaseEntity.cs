using System;
using System.Collections.Generic;
using System.Text;

namespace PhishingPortal.Dto
{
    public class BaseEntity
    {
        public int Id { get; set; }
    }

    public class Auditable : BaseEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
