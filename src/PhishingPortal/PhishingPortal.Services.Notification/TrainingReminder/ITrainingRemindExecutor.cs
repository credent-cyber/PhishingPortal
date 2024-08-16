using PhishingPortal.Common;
using PhishingPortal.Services.Notification.WeeklySummaryReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.TrainingReminder
{
    public interface ITrainingRemindExecutor : IObserver<TrainingRemindInfo>
    {
        IEmailClient EmailSender { get; }
        ILogger<TrainingRemindExecutor> Logger { get; }

        void Start();
        void Stop();
    }
}
