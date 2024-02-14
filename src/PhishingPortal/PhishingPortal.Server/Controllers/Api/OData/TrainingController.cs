using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Repositories;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;
using PhishingPortal.Server.Intrastructure.ActionFilters;

namespace PhishingPortal.Server.Controllers.Api.OData
{
    public class TrainingController : ODataController
    {
        public ILogger<TrainingController> Logger { get; }
        public TenantDbContext DbContext { get; }
        public TrainingController(ILogger<TrainingController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        [ODataAuthorize]
        public IQueryable<Training> Get()
        {
            return DbContext.Training.AsQueryable();
        }
    }
}
