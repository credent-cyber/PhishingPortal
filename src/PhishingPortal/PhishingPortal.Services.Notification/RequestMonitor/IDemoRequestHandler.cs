using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.RequestMonitor
{
    public interface IDemoRequestHandler
    {
        ILogger<DemoRequestHandler> Logger { get; }

        void Start();
        void Stop();
        void Execute();
    }
}
