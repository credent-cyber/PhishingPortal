using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;

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
        public IQueryable<Training> Get()
        {
            return DbContext.Training.AsQueryable();
        }
    }
}
