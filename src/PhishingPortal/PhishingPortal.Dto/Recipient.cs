using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Recipient : BaseEntity
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string WhatsAppNo { get; set; }
        public string Branch { get; set; }
        public string Department { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
        public string LastWorkingDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsADUser { get; set; } = false;


    }

    public class RecipientImport
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Branch { get; set; }
        public string Department { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
        public string ValidationErrMsg { get; private set; }

        public RecipientImport() { }

        public RecipientImport(Recipient o)
        {
            EmployeeCode = o.EmployeeCode;
            Name = o.Name;
            Email = o.Email;
            Mobile = o.Mobile;
            Branch = o.Branch;
            Department = o.Department;
            Address = o.Address;
            DateOfBirth = o.DateOfBirth;
        }

        public void Validate(IEnumerable<TenantDomain> tenantDomains = null)
        {
            ValidationErrMsg = null;

            if (string.IsNullOrEmpty(EmployeeCode) || string.IsNullOrEmpty(Name)
                || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Mobile))
                ValidationErrMsg = "EmployeeCode|Name|Email|Mobile are required field";

            bool isEmail = Regex.IsMatch(Email, AppConfig.EmailRegex);

            if (!isEmail)
                ValidationErrMsg = "Invalid Email Id specified";

            if(tenantDomains != null)
            {
                try
                {
                    var domain = Email.Split("@")[1];
                    if (!tenantDomains.Any(o => o.Domain == domain && o.IsDomainVerified))
                    {
                        ValidationErrMsg = $"Campaign can not be sent to [{domain}] domain";
                    }
                }
                catch { }
            }

            if (Mobile.Length < 10)
                ValidationErrMsg = "Invalid Mobile number specified";

            if(string.IsNullOrEmpty(Mobile))
                ValidationErrMsg = "Recipient Mobile number can't be empty";

            if (!string.IsNullOrEmpty(DateOfBirth) && !DateTime.TryParse(DateOfBirth, out DateTime result))
                ValidationErrMsg = "Invalid date of birth specified, date should be specified as MM/DD/YYYY";

        }

    }


    public enum RecipientSources
    {
        None = 0,
        Csv = 1,
        AzureActiveDirectory = 2,
        OnPremiseAD = 3,
        Imported = 4
    }
}
