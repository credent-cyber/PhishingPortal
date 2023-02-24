using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using PhishingPortal.Domain;

namespace PhishingPortal.Server.Controllers
{
    [Route("[controller]")]
    public class OidcController : Controller
    {
        public SignInManager<PhishingPortalUser> SignInManager { get; }

        public OidcController(SignInManager<PhishingPortalUser> signInManager)
        {
            SignInManager = signInManager;
        }

        [HttpGet]
        [Route("challenge")]
        public IActionResult Challenge([FromQuery] string returnUrl, string provider)
        {
            var redirectUrl = Url.Action(nameof(Callback), "Oidc", new { returnUrl });

            var authProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(authProperties, provider);
        }

        [HttpGet]
        [Route("callback")]
        public async Task<IActionResult> Callback(string returnUrl = "")
        {

            return Redirect(returnUrl);

            //var info = await SignInManager.GetExternalLoginInfoAsync();
            //if (info == null)
            //{
            //    return Redirect("login");
            //}

            //var signInResult = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            //if (signInResult.Succeeded)
            //    return Redirect(returnUrl);
            //else
            //{
            //    return Redirect("forgetpassword");
            //}

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
