using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using PhishingPortal.Domain;
using System.Security.Claims;

namespace PhishingPortal.Server.Controllers
{
    [Route("[controller]")]
    public class OidcController : Controller
    {
        private const string ClaimTypeName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string ClaimTypeEmailAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        public SignInManager<PhishingPortalUser> SignInManager { get; }
        public UserManager<PhishingPortalUser> UserManager { get; }

        public OidcController(SignInManager<PhishingPortalUser> signInManager, UserManager<PhishingPortalUser> userManager)
        {
            SignInManager = signInManager;
            UserManager = userManager;
        }

        [HttpGet]
        [Route("challenge")]
        public IActionResult Challenge([FromQuery] string returnUrl, string provider)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var redirectUrl = Url.Action(nameof(Callback), "Oidc", new { returnUrl });

            var authProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(authProperties, provider);
        }

        [HttpGet]
        [Route("callback")]
        public async Task<IActionResult> Callback(string returnUrl = "")
        {

            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect("/login");
            }

            var name = result?.Principal?.Claims?.FirstOrDefault(o => o.Type == ClaimTypeName)?.Value;
            var email = result?.Principal?.Claims?.FirstOrDefault(o => o.Type == ClaimTypeEmailAddress)?.Value;

            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                var identityResult = await UserManager.CreateAsync(new PhishingPortalUser
                {
                    Email = email,
                    EmailConfirmed = true,
                    UserName = email
                });

                if (identityResult != null && identityResult.Succeeded)
                {
                    user = await UserManager.FindByEmailAsync(email);
                    var r = await UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                }
            }
            else
            {
                var r = await UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            }

            var roleClaim = new Claim("role", "tenantuser");
            var userHasRoleClaim = await UserManager.GetClaimsAsync(user);
            if (!userHasRoleClaim.Any(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value))
            {
                // Add the role claim if it doesn't exist
                var addRoleClaimResult = await UserManager.AddClaimAsync(user, roleClaim);
                
            }

            var signInResult = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
                return Redirect(returnUrl);
            else
            {
                return Redirect("/login");
            }
        }

        [HttpGet]
        [Route("providers")]
        public IActionResult Providers()
        {
            var providers = SignInManager.GetExternalAuthenticationSchemesAsync().Result;

            return Ok(providers.Select(o => $"oidc/challenge?returnUrl={HttpContext.Request.Scheme}://{HttpContext.Request.Host}:{HttpContext.Request.Host.Port}&provider={o.Name}"));
        }

    }
}
