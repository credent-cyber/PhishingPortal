using System;
using System.ComponentModel.DataAnnotations;


namespace PhishingPortal.Dto
{
    public class TemplateIdRequiredIfSmsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (CampaignTemplate)validationContext.ObjectInstance;

            if (model.Type == CampaignType.Sms && string.IsNullOrEmpty(value as string))
            {
                return new ValidationResult(ErrorMessage ?? "This field is Required.", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
    public class CampaignTemplate : Auditable
    {
        [Required(ErrorMessage = "The Template Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Template Type is required.")]
        public CampaignType? Type { get; set; }
        public bool IsHtml { get; set; }
        public bool IsActive { get; set; }
        public string Content { get; set; }
        public string Design { get; set; }
        public string Version { get; set; }

        [TemplateIdRequiredIfSms(ErrorMessage = "Template ID is required")]
        public string TemplateId { get; set; }
    }

    public enum CampaignType
    {
        Email = 1,
        Sms,
        Whatsapp
    }


}
