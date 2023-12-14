using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services;
using PhishingPortal.Server.Services.Interfaces;
using System.Web.Http;

namespace PhishingPortal.Server.Controllers.Api.OData
{

    public class ProvisionController : ODataController
    {
        public ILogger<ProvisionController> Logger { get; }
        public CentralDbContext DbContext { get; }

        public ProvisionController(ILogger<ProvisionController> logger)
        {
            Logger = logger;
            //DbContext = tenantDbResolver;
        }

        [EnableQuery]

        public IQueryable<Tenant> Get()
        {
            return DbContext.Tenants.AsQueryable();
        }
    }
}
