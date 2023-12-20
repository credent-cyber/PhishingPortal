
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.DataContext;
using PhishingPortal.Repositories;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.Dto;
using PhishingPortal.Server.Services.Interfaces;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            var result = from tl in DbContext.TrainingLog.AsQueryable()
                         join r in DbContext.Recipients.AsQueryable() on tl.ReicipientID equals r.Id
                         join tr in DbContext.Training.AsQueryable() on tl.TrainingID equals tr.Id
                         select new TrainingLog
                         {
                             RecipientName = r.Email,
                             Id = tl.Id,
                             ReicipientID = tl.Id,
                             TrainingID = tl.TrainingID,
                             PercentCompleted = tl.PercentCompleted,
                             Status = tl.Status,
                             CreatedBy = tl.CreatedBy,
                             CreatedOn = tl.CreatedOn,
                             SentOn = tl.SentOn,
                             ModifiedOn = tl.ModifiedOn,
                             ModifiedBy = tl.ModifiedBy,
                             TrainingName = tr.TrainingName,
                         };

            return result.AsQueryable();
        }



    }
}
