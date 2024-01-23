using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhishingPortal.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Repositories
{
    public class AppNotificationRepository : IAppNotificationRepository
    {
        public AppNotificationRepository(ILogger logger, PhishingPortalDbContext2 centralDbContext)
        {
            Logger = logger;
            CentralDbContext = centralDbContext;
        }

        public ILogger Logger { get; }

        public PhishingPortalDbContext2 CentralDbContext { get; }

        public void LogCriticalError(Exception ex, string message)
        {
            CentralDbContext.AppLog.Add(new Dto.AppLog
            {
                Message = message,
                ErrorDetail = JsonConvert.SerializeObject(ex),
                CreatedOn = DateTime.Now,
                CreatedBy = nameof(AppNotificationRepository)
            }) ;

            CentralDbContext.SaveChanges();
        }
    }

}
