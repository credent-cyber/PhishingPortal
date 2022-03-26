using PhishingPortal.Dto.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Core
{
    public interface ITenantOnboarding
    {
        /// <summary>
        /// Create a new tenant
        /// </summary>
        /// <param name="name"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        Tenant CreateTenant(string name, string address, string website, string contactEmail, string contactMobile, string uid, string urlPrefix);

        /// <summary>
        /// Upsert tenant configuration data in key/value pair
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void UpsertTenantData(string uid, string key, string value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetTenantDataByKey(string uid, string key);

        /// <summary>
        /// Retrieve all key value configurations for the specified tenant
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        Dictionary<string, string> GetAllTenantData(string uid);
    }
}
