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

namespace PhishingPortal.Server.Controllers.Api.OData
{
    public class TemplateController : ODataController
    {
        public ILogger<TemplateController> Logger { get; }
        public TenantDbContext DbContext { get; }

        public TemplateController(ILogger<TemplateController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        public IQueryable<CampaignTemplate> Get()
        {
            var data = DbContext.CampaignTemplates.OrderByDescending(x => x.Id).AsQueryable();
            return data;
        }


    }
}
