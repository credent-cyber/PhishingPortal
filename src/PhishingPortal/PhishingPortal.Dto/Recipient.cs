using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public void Validate()
        {
            ValidationErrMsg = string.Empty;

            if (string.IsNullOrEmpty(EmployeeCode) || string.IsNullOrEmpty(Name)
                || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Mobile))
                ValidationErrMsg = "EmployeeCode|Name|Email|Mobile are required field";

            bool isEmail = Regex.IsMatch(Email, AppConfig.EmailRegex);

            if (!isEmail)
                ValidationErrMsg = "Invalid Email Id specified";

            if (Mobile.Length <= 10)
                ValidationErrMsg = "Invalid Mobile number specified";

            if (!string.IsNullOrEmpty(DateOfBirth) && !DateTime.TryParse(DateOfBirth, out DateTime result))
                ValidationErrMsg = "Invalid date of birth specified, date should be specified as MM/DD/YYYY";
        }

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
