using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Controllers.Api
{
    public class CampaignController : ODataController
    {
        public ILogger<CampaignController> Logger { get; }
        public TenantDbContext DbContext { get; }

        public CampaignController(ILogger<CampaignController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        public IQueryable<Campaign> Get()
        {
            return DbContext.Campaigns.AsQueryable();
        }
    }
}
