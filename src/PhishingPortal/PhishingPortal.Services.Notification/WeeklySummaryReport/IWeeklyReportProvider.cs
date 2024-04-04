using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public interface IWeeklyReportProvider : IReportProvider, IObservable<WeeklyReportInfo> { }

}
