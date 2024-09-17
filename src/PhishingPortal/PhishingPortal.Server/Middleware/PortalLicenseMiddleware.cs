using Duende.IdentityServer.Extensions;
using PhishingPortal.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using PhishingPortal.Dto.Auth;
namespace PhishingPortal.Server.Middleware
{
    /// <summary>
    /// This middleware is meant for proper subscription / license for the tenant
    /// </summary>
    public class PortalLicenseMiddleware : IMiddleware
    {
        private readonly ILicenseService licenseServices;
        private readonly SignInManager<PhishingPortalUser> signInManager;

        public PortalLicenseMiddleware(ILicenseService licenseServices, SignInManager<PhishingPortalUser> signInManager)
        {
            this.licenseServices = licenseServices;
            this.signInManager = signInManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //var isvalidLicense = false;

            ////if (context.Request.Path.Value.Contains("nosubscription"))
            ////    await next(context);

            //if (context.User.IsAuthenticated())
            //{
            //    var isAdmin = context.IsSuperAdmin();
            //    if (!isAdmin)
            //    {
            //        var tenantIdentifier = context.User.Claims.FirstOrDefault(o => o.Type == "tenant")?.Value ?? string.Empty;

            //        if (!string.IsNullOrEmpty(tenantIdentifier))
            //        {
            //            var valid = await licenseServices.HasValidLicense(tenantIdentifier);

            //            context.Response.StatusCode = 302;
            //            context.Response.Redirect("/nosubscription");
            //            return;
            //        }

            //    }
            //    // TODO: we may create an expiry page
            //    // check if expiry is near send app notification or reflect somewher
            //}

            //await next(context);

            throw new NotImplementedException();
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
