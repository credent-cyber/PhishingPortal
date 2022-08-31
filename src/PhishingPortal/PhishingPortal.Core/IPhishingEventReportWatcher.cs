using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Core
{
    public interface IPhishingEventReportWatcher
    {
        Task<Dictionary<string,string>> CheckAsync();
    }
}
