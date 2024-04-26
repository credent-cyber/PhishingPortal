using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Intrastructure.ActionFilters;
using PhishingPortal.Server.Services.Interfaces;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace PhishingPortal.Server.Controllers.Api.OData
{
    public class CampaignlogController : ODataController
    {
        public ILogger<CampaignlogController> Logger { get; }
        public TenantDbContext DbContext { get; }
        public CampaignlogController(ILogger<CampaignlogController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        [ODataAuthorize]
        public IQueryable<CampaignLog> Get()
        {
            return DbContext.CampaignLogs.AsQueryable();
        }

    }
}
