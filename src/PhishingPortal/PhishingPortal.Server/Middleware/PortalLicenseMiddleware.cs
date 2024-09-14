using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PhishingPortal.Common;
using PhishingPortal.Dto;
using PhishingPortal.Licensing;

using PhishingPortal.Server;
using PhishingPortal.UI.Blazor.Client;
using PhishingPortal.UI.Blazor.Services;
using System.Net.Mime;

namespace PhishingPortal.Server.Middleware
{
    /// <summary>
    /// This middleware is meant for proper subscription / license for the tenant
    /// </summary>

    public class PortalLicenseMiddleware : IMiddleware
    {
        private readonly ILicenseProvider licenseProvider;
        private readonly TenantClient tenantClient;
        private Dictionary<string, string> settings = new();

        public PortalLicenseMiddleware(ILicenseProvider licenseProvider, TenantClient tenantClient)
        {
            this.licenseProvider = licenseProvider;
            this.tenantClient = tenantClient;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var isvalidLicense = false;

            if (context.User.IsAuthenticated())
            {
               // settings = await tenantClient.GetSubscription();
            }

            //if (settings.Count <= 0 && context.User.IsAuthenticated() && !context.IsSuperAdmin())
            //    settings = await tenantClient.GetSettings();

            //// retrieve license and validate

            //if (settings != null && settings.Count > 0)
            //{
            //    var licenseSetting = settings[Constants.Keys.LICENSE];

            //    if (licenseSetting != null)
            //    {
            //        LicenseInfo? lic = JsonConvert.DeserializeObject<LicenseInfo>(licenseSetting);

            //        if (lic != null)
            //        {
            //            var result = this.licenseProvider.Validate(lic.Content, lic.PublicKey);

            //            isvalidLicense = result.Valid;
            //        }
            //    }

            //}

            //if (!isvalidLicense)
            //    context.Response.Redirect("/Account/Login");

            await next(context);
        }
    }


    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PortalLicenseMiddlewareExtensions
    {
        public static IApplicationBuilder UsePortalLicensing(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PortalLicenseMiddleware>();
        }
    }

}
