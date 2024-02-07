using Microsoft.AspNetCore.Authentication;

namespace PhishingPortal.Server.Middleware
{
    public static class AdAuthAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="AdAuthAppBuilderExtensions"/> to the specified <see cref="IApplicationBuilder"/>, which enables authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseAdAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<AdAuthenticationMiddleware>();
        }
    }

}
