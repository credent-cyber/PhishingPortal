﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public interface IReportProvider
    {
        Task CheckAndPublish(CancellationToken stopppingToken);
    }
}
