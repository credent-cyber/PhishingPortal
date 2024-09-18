using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using PhishingPortal.Common;

namespace PhishingPortal.Dto
{
    public class AzureRegistrationSettings
    {
        [Required]
        public string ClientID { get; set; }
        [Required]
        public string ClientSecret { get; set; }
        [Required]
        public string TenantID { get; set; }

        public AzureRegistrationSettings()
        {
            ClientID = string.Empty;
            ClientSecret = string.Empty;
            TenantID = string.Empty;
        }

        public AzureRegistrationSettings(string clientId, string clientSecret, string tenantID)
        {
            ClientID = clientId;
            ClientSecret = clientSecret;
            TenantID = tenantID;
        }

        public AzureRegistrationSettings(Dictionary<string, string> values)
        {
            ClientID = values[Constants.Keys.ClIENT_ID] ?? String.Empty;
            ClientSecret = values[Constants.Keys.CLIENT_SECRET] ?? String.Empty;
            TenantID = values[Constants.Keys.TENANT_ID] ?? String.Empty;
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            if(ClientID != null)
                result.Add(Constants.Keys.ClIENT_ID, ClientID);

            if(ClientSecret != null)
                result[Constants.Keys.CLIENT_SECRET] = ClientSecret;

            if(TenantID != null)
                result[Constants.Keys.TENANT_ID] = TenantID;

            return result;

        }
    }

    public class GoogleRegistrationSettings
    {
        [Required]
        public string GoogleClientID { get; set; }
        [Required]
        public string GoogleClientSecret { get; set; }

        public GoogleRegistrationSettings()
        {
            GoogleClientID = string.Empty;
            GoogleClientSecret = string.Empty;
        }

        public GoogleRegistrationSettings(string clientId, string clientSecret, string tenantID)
        {
            GoogleClientID = clientId;
            GoogleClientSecret = clientSecret;
        }

        public GoogleRegistrationSettings(Dictionary<string, string> values)
        {
            GoogleClientID = values[Constants.Keys.GOOGLE_ClIENT_ID] ?? String.Empty;
            GoogleClientSecret = values[Constants.Keys.GOOGLE_CLIENT_SECRET] ?? String.Empty;
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            if (GoogleClientID != null)
                result.Add(Constants.Keys.GOOGLE_ClIENT_ID, GoogleClientID);

            if (GoogleClientSecret != null)
                result[Constants.Keys.GOOGLE_CLIENT_SECRET] = GoogleClientSecret;

            return result;

        }
    }

    public class OnPromiseADSettings
    {
        [Required]
        public string Domain { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Username { get; set; }

        public OnPromiseADSettings()
        {
            Domain = string.Empty;
            Password = string.Empty;
            Username = string.Empty;
        }

        public OnPromiseADSettings(string clientId, string clientSecret, string tenantID)
        {
            Domain = clientId;
            Password = clientSecret;
            Username = tenantID;
        }

        public OnPromiseADSettings(Dictionary<string, string> values)
        {
            Domain = values[Constants.Keys.OnPromiseAD_Doamin] ?? String.Empty;
            Password = values[Constants.Keys.OnPromiseAD_Password] ?? String.Empty;
            Username = values[Constants.Keys.OnPromiseAD_Username] ?? String.Empty;
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            if (Domain != null)
                result.Add(Constants.Keys.OnPromiseAD_Doamin, Domain);

            if (Password != null)
                result[Constants.Keys.OnPromiseAD_Password] = Password;

            if (Username != null)
                result[Constants.Keys.OnPromiseAD_Username] = Username;

            return result;

        }

       
    }

    public class ApplicationSettings
    {
        public bool IsTrainingReminderEnabled { get; set; }
        public bool IsWeeklyReportEnabled { get; set; }

        [RequiredIfReportEnable(ErrorMessage = "This is required")]
        public string WeeklyReportRecipients { get; set; }

        public ApplicationSettings()
        {
            IsTrainingReminderEnabled = false;
            IsWeeklyReportEnabled = false;
            WeeklyReportRecipients = string.Empty;
        }

        public ApplicationSettings(bool isTrainingReminderEnabled,bool isWeeklyReportEnabled, string weeklyReportRecipients)
        {
            IsTrainingReminderEnabled = isTrainingReminderEnabled;
            IsWeeklyReportEnabled = isWeeklyReportEnabled;
            WeeklyReportRecipients = weeklyReportRecipients;
        }
        public ApplicationSettings(Dictionary<string, string> values)
        {
            IsTrainingReminderEnabled = bool.Parse(values.GetValueOrDefault(Constants.Keys.TrainingReminder_IsEnabled, "false"));
            IsWeeklyReportEnabled = bool.Parse(values.GetValueOrDefault(Constants.Keys.WeeklyReport_IsEnabled, "false"));
            WeeklyReportRecipients = values.GetValueOrDefault(Constants.Keys.WeeklyReport_Recipients, String.Empty);
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            result[Constants.Keys.TrainingReminder_IsEnabled] = IsTrainingReminderEnabled.ToString();
            result[Constants.Keys.WeeklyReport_IsEnabled] = IsWeeklyReportEnabled.ToString();
            result[Constants.Keys.WeeklyReport_Recipients] = WeeklyReportRecipients;

            return result;
        }
    }

    public class RequiredIfReportEnableAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (ApplicationSettings)validationContext.ObjectInstance;

            if (model.IsWeeklyReportEnabled == true && string.IsNullOrEmpty(value as string))
            {
                return new ValidationResult(ErrorMessage ?? "This field is Required.", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
