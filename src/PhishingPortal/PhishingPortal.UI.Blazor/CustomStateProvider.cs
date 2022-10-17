using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.BouncyCastle.Asn1.Mozilla;
using PhishingPortal.Dto.Auth;
using PhishingPortal.UI.Blazor.Services;

namespace PhishingPortal.UI.Blazor;

public class CustomStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService api;
    private CurrentUser _currentUser;
    public CustomStateProvider(IAuthService api)
    {
        this.api = api;
        _currentUser = new CurrentUser();
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            var userInfo = await GetCurrentUser();
            if (userInfo.IsAuthenticated)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, _currentUser.UserName) }
                                .Concat(_currentUser.Claims.Select(c => new Claim(c.Key, c.Value))).ToList();

                var roleClaims = _currentUser.Claims.Where(c => c.Key == "role");

                foreach(var rc in roleClaims)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rc.Value));
                }
                
                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request failed:" + ex.ToString());
        }
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
    private async Task<CurrentUser> GetCurrentUser()
    {
        if (_currentUser != null && _currentUser.IsAuthenticated) return _currentUser;
        _currentUser = await api.CurrentUserInfo();
        return _currentUser;
    }
    public async Task Logout()
    {
        await api.Logout();
        _currentUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task Login(LoginRequest loginParameters)
    {
        await api.Login(loginParameters);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<(bool, string)> ForgetPassword(ForgetPasswordRequest request)
    {
        var success = await api.ForgetPassword(request);
        var message = success ? "We have sent a password reset link to you respective email address, please check and follow the instructions" : "There was a problem accepting your request, please try again";
        return await Task.FromResult((success, message));
    }

    public async Task<(bool, string)> ResetPassword(ResetPasswordRequest request)
    {
        var success = await api.ResetPassword(request);
        var message = success ? "Your password has been successfully reset." : "There was a problem reseting your password, please try again";
        return await Task.FromResult((success, message));
    }
}
