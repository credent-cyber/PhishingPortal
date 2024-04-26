using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.OnPremiseAD
{
    public class ADApplicationSetting
    {
        public bool EnableSyncADUsersDetail { get; set; } = true;
        public int WaitIntervalInMinutes { get; set; }
        public ADApplicationSetting(IConfiguration config)
        {
            config.GetSection("WorkerSettings").Bind(this);
        }
    }
}
