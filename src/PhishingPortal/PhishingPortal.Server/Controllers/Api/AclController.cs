using DocumentFormat.OpenXml.Office2010.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NPOI.SS.Formula.Functions;
using PhishingPortal.Domain;
using PhishingPortal.Dto.Auth;
using PhishingPortal.Server.Controllers.Api.Abstraction;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace PhishingPortal.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AclController : BaseApiController
    {
        private readonly UserManager<PhishingPortalUser> _userManager;
        private readonly SignInManager<PhishingPortalUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AclController(UserManager<PhishingPortalUser> userManager, SignInManager<PhishingPortalUser> signInManager,
            IEmailSender emailSender, ILogger<AclController> logger) : base(logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null) return BadRequest("User does not exist");
            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!singInResult.Succeeded) return BadRequest("Invalid password");
            await _signInManager.SignInAsync(user, request.RememberMe);
            return Ok();
        }

        [Authorize]
        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet]
        [Route("currentUserInfo")]
        public CurrentUser CurrentUserInfo()
        {
            var user = new CurrentUser
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                UserName = User?.Identity?.Name ?? "guest",
                Claims = User?.Claims?
                              .ToDictionary(c => c.Type, c => c.Value)
            };

            if (!User.Claims.Any(o => o.Type == "role"))
            {
                user.Claims.Add(ClaimTypes.Role, "tenantuser");
            }

            return user;
        }

        [Route("forgetpassword")]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                Logger.LogError($"{request.Email} - user not found or email is not confirmed");
                return BadRequest();
            }

            try
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = $"{Request.Scheme}://{Request.Host.Host}:{(Request.Host.Port != 80 ? Request.Host.Port.ToString() : string.Empty)}/resetpassword/{code}";

                if (callbackUrl == null)
                    return BadRequest();

                await _emailSender.SendEmailAsync(
                    request.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }

            return Ok();
        }


        [Route("resetpassword")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                Logger.LogError($"{request.Code} - user not found or email is not confirmed");
                return BadRequest();
            }

            var code = WebEncoders.Base64UrlDecode(request.Code);
            var scode = Encoding.UTF8.GetString(code);

            var result = await _userManager.ResetPasswordAsync(user, scode, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.FirstOrDefault());

            await _emailSender.SendEmailAsync(
                user.Email,
                "Password Reset Successfully",
                $"Your password was reset successfully");

            return Ok();
        }


    }
}
