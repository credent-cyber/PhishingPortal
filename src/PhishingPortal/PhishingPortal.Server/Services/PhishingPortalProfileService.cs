using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using PhishingPortal.Domain;
using System.Security.Claims;

namespace PhishingPortal.Server.Services
{
    public class PhishingPortalProfileService : IProfileService
    {
        public PhishingPortalProfileService(UserManager<PhishingPortalUser> userManager,
            IUserClaimsPrincipalFactory<PhishingPortalUser> claimsFactory)
        {
            UserManager = userManager;
            ClaimsFactory = claimsFactory;
        }

        public UserManager<PhishingPortalUser> UserManager { get; }
        public IUserClaimsPrincipalFactory<PhishingPortalUser> ClaimsFactory { get; }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await UserManager.FindByIdAsync(sub);
            var principal = await ClaimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            claims.Add(new Claim("tenant_id", "T-20220619003439" ?? string.Empty));

            context.IssuedClaims = claims;

            await Task.CompletedTask;
        }
    }

}
