using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Utilities.WeeklyReport
{
    public interface IWeeklyReportExecutor : IObserver<EmailCampaignInfo>
    {
        IEmailClient EmailSender { get; }

        void Start();
        void Stop();
    }
}
