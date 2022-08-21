using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhishingPortal.Core
{
    public interface IPhishingEventReportProcessor
    {
        Task<int> ProcessAsync();
    }
}
