using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Repositories;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PhishingPortal.Server.Controllers.Api.OData
{
    public class MyTrainingController : ODataController
    {
        public ILogger<MyTrainingController> Logger { get; }
        public TenantDbContext DbContext { get; }
        public MyTrainingController(ILogger<MyTrainingController> logger, ITenantDbResolver tenantDbResolver)
        {
            Logger = logger;
            DbContext = tenantDbResolver.TenantDbCtx;
        }

        [EnableQuery]
        public IQueryable<MyTraining> Get()
        {
            var userEmail = HttpContext.GetUserEmail();

            var query = from tl in DbContext.TrainingLog
                   join t in DbContext.Training on tl.TrainingID equals t.Id
                   join r in DbContext.Recipients on tl.ReicipientID equals r.Id
                   where r.Email == userEmail
                   select new MyTraining
                   {
                       Id = tl.Id,
                       Training = t,
                       TrainingLog = tl
                   };
            
            return query;

        }     
    }
}
