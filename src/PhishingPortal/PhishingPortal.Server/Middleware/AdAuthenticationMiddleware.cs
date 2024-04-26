using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using NuGet.Packaging;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using System.Security.Claims;

namespace PhishingPortal.Server.Middleware
{

    public class AdAuthenticationMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdAuthenticationMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthenticationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next item in the middleware pipeline.</param>
        /// <param name="schemes">The <see cref="IAuthenticationSchemeProvider"/>.</param>
        public AdAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes, ILogger<AdAuthenticationMiddleware> logger)
        {
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(schemes);

            _next = next;

            Schemes = schemes;
            logger = logger;
        }

        public IAuthenticationSchemeProvider Schemes { get; set; }

        /// <summary>
        /// Invokes the middleware performing authentication.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                {
                    OriginalPath = context.Request.Path,
                    OriginalPathBase = context.Request.PathBase
                });

                // Give any IAuthenticationRequestHandler schemes a chance to handle the request
                var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

                foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
                {
                    var handler = await handlers.GetHandlerAsync(context, scheme.Name) as IAuthenticationRequestHandler;
                    if (handler != null && await handler.HandleRequestAsync())
                    {
                        return;
                    }
                }

                var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();

                if (defaultAuthenticate != null)
                {
                    var result = await context.AuthenticateAsync(defaultAuthenticate.Name);

                    if (result?.Principal != null)
                    {
                        context.User = result.Principal;

                        IList<Claim> identityClaims = context?.User?.Claims?.ToList();

                        var userName = context.User?.Identity?.Name ?? "guest";
                        var isAuthenticated = false;

                        using (var scope = context.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                        {
                            var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<PhishingPortalUser>>();

                            var identityUser = await _userManager.Users
                                .FirstOrDefaultAsync(o => o.ActiveDirectoryUsername.ToLower() == userName.ToLower());

                            if (identityUser != null)
                            {
                                isAuthenticated = true;

                                try
                                {
                                    var dbClaims = await _userManager.GetClaimsAsync(identityUser);

                                    identityClaims.AddRange(dbClaims);

                                    var nameClaim = identityClaims.FirstOrDefault(o => o.Type == ClaimTypes.Name);

                                    if (nameClaim != null)
                                        identityClaims.Remove(nameClaim);

                                    identityClaims.Add(new Claim(ClaimTypes.Name, identityUser.Email));

                                    if (!identityClaims.Any(o => o.Type == "role"))
                                    {
                                        identityClaims.Add(new Claim(ClaimTypes.Role, "tenantuser"));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogCritical(ex, ex.Message);
                                    throw;
                                }
                            }
                        }

                        var identity = new ClaimsIdentity(identityClaims, result.Principal.Identity.AuthenticationType);

                        var principal = isAuthenticated ? new ClaimsPrincipal(identity) : new ClaimsPrincipal(new ClaimsIdentity());
                        context.User = principal;

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
                throw;
            }

            await _next(context);
        }
    }

}
