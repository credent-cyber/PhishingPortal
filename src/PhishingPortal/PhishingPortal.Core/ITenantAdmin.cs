using PhishingPortal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Core
{
    public interface ITenantAdmin
    {
        /// <summary>
        /// Provision a new tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        Task<ApiResponse<Tenant>> CreateAsync(Tenant tenant);
        
    }
}
