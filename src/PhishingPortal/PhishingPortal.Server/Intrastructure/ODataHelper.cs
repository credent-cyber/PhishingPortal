namespace PhishingPortal.Server.Intrastructure
{
    using Microsoft.AspNetCore.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.ModelBuilder;
    using PhishingPortal.Dto;
    using PhishingPortal.Server.Intrastructure.ActionFilters;

    [ODataAuthorize]
    public static class ODataHelper
    {

        public static IMvcBuilder AddODataControllers(this IMvcBuilder builder)
        {
            return builder.AddOData(option =>
            {
                option.Select();
                option.Expand();
                option.Filter();
                option.OrderBy();
                option.Count();
                option.SetMaxTop(100);
                option.SkipToken();
                option.AddRouteComponents("Odata", GetModel());
            });
        }
        
        private static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<CampaignTemplate>("Template");
            builder.EntitySet<CampaignLog>("Campaignlog");
            builder.EntitySet<Campaign>("Campaign");
            builder.EntitySet<Training>("Training");
            builder.EntitySet<TrainingLog>("Traininglog");
            builder.EntitySet<TrainingQuizQuestion>("TrainingQuiz");
            builder.EntitySet<MyTraining>("MyTraining");
            builder.EntitySet<Tenant>("Tenant");
            return builder.GetEdmModel();
        }
    }


}
