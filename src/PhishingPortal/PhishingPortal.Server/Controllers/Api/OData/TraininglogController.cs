using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Controllers.Api.OData
{
    public class TraininglogController : ODataController
    {
        public ILogger<TraininglogController> Logger { get; }
        public TenantDbContext DbContext { get; }
        public TraininglogController(ILogger<TraininglogController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        public IQueryable<TrainingLog> Get()
        {
            return DbContext.TrainingLog.AsQueryable();
        }
    }
}
