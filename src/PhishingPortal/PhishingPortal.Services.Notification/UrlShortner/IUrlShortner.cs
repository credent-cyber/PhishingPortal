using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.UrlShortner
{
    public interface IUrlShortner
    {
        Task<string> CallApiAsync(string urlToShorten);
    }
}
