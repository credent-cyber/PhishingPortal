using Newtonsoft.Json;
using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PhishingPortal.Dto.RequiredIfAttribute;

namespace PhishingPortal.Dto
{
    public class Campaign : Auditable
    {
        [Required]
        public string Name { get; set; }

        [RequiredIfString("Detail.Type", new[] { CampaignType.Email }, "The Description field is required.")]
        public string Description { get; set; }

        [RequiredIfString("Detail.Type", new[] { CampaignType.Email }, "The Subject field is required.")]
        public string Subject { get; set; }


        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Category { get; set; }

        public CampaignStateEnum State { get; set; }

        public bool IsActive { get; set; }

        public virtual CampaignDetail Detail { get; set; }

        public int CampaignScheduleId { get; set; }

        [ForeignKey("CampaignScheduleId")]
        public virtual CampaignSchedule Schedule { get; set; }

        [RequiredIfString("Detail.Type", new[] { CampaignType.Email }, "The ReturnUrl field is required")]
        public string ReturnUrl { get; set; }



        [RequiredIf("Detail.Type", new[] { CampaignType.Email }, "The sender field is required")]
        [Required(ErrorMessage = "The sender field is required.")]
        public string FromEmail { get; set; }
    }




    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private string otherPropertyPath;
        private CampaignType[] otherPropertyValues;
        private string errorMessage;

        public RequiredIfAttribute(string otherPropertyPath, CampaignType[] otherPropertyValues, string errorMessage)
        {
            this.otherPropertyPath = otherPropertyPath;
            this.otherPropertyValues = otherPropertyValues;
            this.errorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherPropertyPathParts = otherPropertyPath.Split('.');
            var otherPropertyValue = GetNestedPropertyValue(validationContext.ObjectInstance, otherPropertyPathParts);

            if (otherPropertyValue == null || !otherPropertyValues.Contains((CampaignType)otherPropertyValue))
            {
                return ValidationResult.Success;
            }

            if (string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult(this.errorMessage);
            }

            if ((CampaignType)otherPropertyValue == CampaignType.Sms || (CampaignType)otherPropertyValue == CampaignType.Whatsapp)
            {
                if (!IsValidPhoneNumber(value.ToString()))
                {
                    return new ValidationResult("Please enter a valid phone number.", new[] { validationContext.MemberName });
                }
            }
            else if (validationContext.MemberName != "Description" && validationContext.MemberName != "Subject" && validationContext.MemberName != "ReturnUrl")
            {
                if (!IsValidEmail(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? "Please enter a valid email address", new[] { validationContext.MemberName });
                }
            }

            return ValidationResult.Success;
        }


        private static object GetNestedPropertyValue(object obj, string[] propertyPathParts)
        {
            if (propertyPathParts.Length == 1)
            {
                var propertyInfo = obj.GetType().GetProperty(propertyPathParts[0]);
                if (propertyInfo == null)
                {
                    return null;
                }

                return propertyInfo.GetValue(obj);
            }
            else
            {
                var propertyInfo = obj.GetType().GetProperty(propertyPathParts[0]);
                if (propertyInfo == null)
                {
                    return null;
                }

                var nestedObject = propertyInfo.GetValue(obj);
                if (nestedObject == null)
                {
                    return null;
                }

                return GetNestedPropertyValue(nestedObject, propertyPathParts.Skip(1).ToArray());
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.Match(phoneNumber, @"^(\+[0-9]{1,3})?[0-9]{10}$").Success;
        }
    }

    public class RequiredIfStringAttribute : RequiredIfAttribute
    {
        public RequiredIfStringAttribute(string otherPropertyPath, CampaignType[] otherPropertyValues, string errorMessage)
            : base(otherPropertyPath, otherPropertyValues, errorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var campaign = validationContext.ObjectInstance as Campaign;
            if (campaign != null && campaign.Detail?.Type == CampaignType.Email && value == null)
                //if (value != null && !(value is string))
            {
                return new ValidationResult(ErrorMessage ?? "This field is Required.", new[] { validationContext.MemberName });               
            }

            return base.IsValid(value, validationContext);
        }
    }



}
