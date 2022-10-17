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

    }
}
