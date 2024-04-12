using PhishingPortal.Common;
using PhishingPortal.Services.Notification.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public interface IWeeklyReportExecutor : IObserver<WeeklyReportInfo>
    {
        IEmailClient EmailSender { get; }
        ILogger<WeeklyReportExecutor> Logger { get; }

        void Start();
        void Stop();
    }
}
