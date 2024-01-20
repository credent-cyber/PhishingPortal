using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class OnPremiseADUsers : BaseEntity
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string WhatsAppNo { get; set; }
        public string Branch { get; set; }
        public string Department { get; set; }
        public string Address { get; set; }
        public DateTime ? DateOfBirth { get; set; }
        public string LastWorkingDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
