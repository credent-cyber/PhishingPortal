﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.RequestMonitor
{
    public interface IRequestEmailSender
    {
        public Task ExecuteTask(string eml, string cmny);
    }
}
