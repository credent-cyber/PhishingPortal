using PhishingPortal.Dto.Auth;

namespace PhishingPortal.UI.Blazor.Services
{
    public interface IAuthService
    {
        Task<CurrentUser> CurrentUserInfo();
        Task<bool> ForgetPassword(ForgetPasswordRequest param);
        Task<bool> ResetPassword(ResetPasswordRequest param);
        Task Login(LoginRequest loginRequest);
        Task Logout();
    }
}
