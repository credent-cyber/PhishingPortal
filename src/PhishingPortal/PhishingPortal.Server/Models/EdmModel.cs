using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using PhishingPortal.Dto;

namespace PhishingPortal.Server.Models
{
    public class EdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<CampaignTemplate>("campaignTemplate");

            return builder.GetEdmModel();
        }
    }
}
