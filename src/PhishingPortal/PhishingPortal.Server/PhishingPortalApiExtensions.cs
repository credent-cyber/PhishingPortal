using Duende.IdentityServer.Extensions;
using PhishingPortal.Server;
using System.Security.Claims;

namespace PhishingPortal.Server
{
    public static class PhishingApiExtensions
    {

        public static string GetCurrentUser(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("Context not found");

            var name = httpContext.User.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Name);

            if (name == null)
                throw new ArgumentException("User cannot be resolved");

            return name.Value;
        }

        public  static string GetUserEmail(this HttpContext context)
        {
            if (context.User.IsAuthenticated())
            {
                return context?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            }

            throw new InvalidOperationException("The use is not authentication");
        }

        public static string GetTenantIdentifier(this HttpContext context)
        {
            if (context.User.IsAuthenticated())
            {
                return context?.User?.Claims?.FirstOrDefault(c => c.Type == "tenant")?.Value ?? string.Empty;
            }

            throw new InvalidOperationException("The use is not authentication");
        }

        public static bool IsSuperAdmin(this HttpContext context)
        {
            if (context.User.IsAuthenticated())
            {
                return context?.User?.Claims?.Any(c => c.Type == "role" && c.Value == "superadmin") ?? false;
            }

            return false;
        }

    }
}
