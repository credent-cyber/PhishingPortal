using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Repositories;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Controllers.Api.OData
{
    public class TrainingQuizController : ODataController
    {
        public ILogger<TrainingQuizController> Logger { get; }
        public TenantDbContext DbContext { get; }
        public TrainingQuizController(ILogger<TrainingQuizController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        public IQueryable<TrainingQuiz> Get()
        {
            return DbContext.TrainingQuiz.AsQueryable();
        }

    }
}
