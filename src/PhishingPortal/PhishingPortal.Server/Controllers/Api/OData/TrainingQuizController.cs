using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Repositories;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Server.Intrastructure.ActionFilters;

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
        [ODataAuthorize]
        public IQueryable<TrainingQuizQuestion> Get()
        {
           // return DbContext.TrainingQuiz.Include(o=>o.TrainingQuizAnswer).GroupBy(x => x.TrainingId).Select(x => x.First()).ToList().AsQueryable();
            return DbContext.TrainingQuizQuestion.Include(o=>o.TrainingQuizAnswer).AsQueryable();
        }

    }
}
