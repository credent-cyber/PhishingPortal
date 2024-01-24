using PhishingPortal.Dto;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.OnPremiseAD.ActiveDirectory
{
    public interface ISyncUsersDetails
    {
        Task Sync(CancellationToken cancellationToken);

    }
}
