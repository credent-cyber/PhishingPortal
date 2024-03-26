using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Utilities.WeeklyReport
{
    public interface IWeeklySummaryReports
    {
        IEmailClient EmailSender { get; }

        void Start();
        void Stop();
    }
}
