using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace PhishingPortal.UI.Blazor
{
    public class PhishingPortalAuthState : RemoteAuthenticationState {

    }

    public class AuthState
    {
        public ILogger<AuthState> Logger { get; }
        public AuthenticationStateProvider AuthenticationStateProvider { get; }
        public bool IsAuthenticated { get; set; }
        public bool IsTenant { get; set; }
        public string TenantIdentifier { get; set; } = String.Empty;
        public ClaimsPrincipal CurrentUser { get; set; } = new ClaimsPrincipal();

        public AuthState(ILogger<AuthState> logger, AuthenticationStateProvider authenticationStateProvider)
        {
            Logger = logger;

            AuthenticationStateProvider = authenticationStateProvider;
            
            var authStateTaskResult = AuthenticationStateProvider.GetAuthenticationStateAsync().Result;
            
            CurrentUser = authStateTaskResult.User;
            
            if (authStateTaskResult.User.Identity != null)
            {
                CurrentUser = authStateTaskResult.User;
                IsAuthenticated = CurrentUser.Identity.IsAuthenticated;
                IsTenant = CurrentUser.IsInRole("tenant");
                TenantIdentifier = CurrentUser?.Claims?.FirstOrDefault(o => o.Type == "tenant")?.Value ?? String.Empty;
            }
            else
            {
                IsAuthenticated = false;
                TenantIdentifier = String.Empty;
                IsTenant = false;
            }
        }
    }
}
