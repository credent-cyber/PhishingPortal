using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services.Interfaces;
using System.Web.Http;

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

        public IQueryable<CampaignLog> Get()
        {
            var data = DbContext.CampaignLogs.AsQueryable();
            return data;
        }

    }
}
