using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;

namespace PhishingPortal.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnboardingController : BaseApiController
    {

        public OnboardingController(ILogger<OnboardingController> logger, ITenantAdminRepository tenantAdminRepo, INsLookupHelper nsLookup,
            UserManager<PhishingPortalUser> userManager) : base(logger)
        {
            tenatAdminRepo = tenantAdminRepo;
            NsLookup = nsLookup;
            UserManager = userManager;
        }

        public ITenantAdminRepository tenatAdminRepo { get; }
        public INsLookupHelper NsLookup { get; }
        public UserManager<PhishingPortalUser> UserManager { get; }

        [HttpPost]
        [Route("Register")]
        public async Task<Tenant> Register(Tenant tenant)
        {
            return await tenatAdminRepo.CreateTenantAsync(tenant);
        }

        [HttpGet]
        public async Task<List<Tenant>> GetAll(int pageIndex, int pageSize)
        {
            return await tenatAdminRepo.GetAllAsync(pageIndex, pageSize);
        }

        [HttpGet]
        [Route("TenantByUniqueId")]
        public async Task<Tenant> GetByUniqueId(string uniqueId)
        {
            return await tenatAdminRepo.GetByUniqueId(uniqueId);
        }

        [HttpPost]
        [Route("Provision")]
        public async Task Provision(ProvisionTenantRequest request)
        {
            var result = await tenatAdminRepo.ProvisionAsync(request.TenantId, request.ConnectionString);

            if (result == false)
                NotFound("Tenant not registered yet");
        }

        [HttpPost]
        [Route("Confirm")]
        [AllowAnonymous]
        public async Task<ApiResponse<Tenant>> Confirm(TenantConfirmationRequest request)
        {
            var response = new ApiResponse<Tenant>();
            Tenant t;
            try
            {

                t = await tenatAdminRepo.ConfirmRegistrationAsync(request.UniqueId, request.RegisterationHash, request.Url);
                response.IsSuccess = true;
                response.Message = "Registration confirmed, please proceed with domain registration";
                response.Result = t;
            }
            catch (Exception ex)
            {
                Logger.LogInformation(ex, $"TenantUniqueId: {request.UniqueId}, Error: {ex.Message}");
                return new ApiResponse<Tenant>() { Message = ex.Message };
            }

            return response;
        }

        [HttpPost]
        [Route("ConfirmDomain")]
        [AllowAnonymous]
        public async Task<ApiResponse<Tenant>> ConfirmDomain(DomainVerificationRequest domain)
        {
            var response = new ApiResponse<Tenant>();
            Tenant t;
            try
            {
                // do actual domain verification
                if (NsLookup.VerifyDnsRecords("TXT", domain.Domain.Trim().ToLower(), domain.DomainVerificationCode))
                {
                    t = await tenatAdminRepo.ConfirmDomainAsync(domain);
                    response.IsSuccess = true;
                    response.Message = "Sucessfully verified domain";
                    response.Result = t;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Domain verification failed as DNS record lookup failed, please try again";
                }

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"TenantUniqueId: {domain.UniqueId}, Error: {ex.Message}");
                return new ApiResponse<Tenant>() { Message = ex.Message };
            }

            return response;
        }


        [HttpPost]
        [Route("CreateDefaultUser")]
        [AllowAnonymous]
        public async Task<ApiResponse<bool>> CreateDefaultUser(TenantAdminUser user)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var domain = user.Email.Split("@")[1];
                var tenant = await tenatAdminRepo.GetByDomain(domain);
                
                if (tenant.UniqueId != user.TenantUniqueId)
                    throw new Exception("Invalid request");

                if (user.ConfirmPassword != user.Password)
                    throw new Exception("Confirmed password didn't match with Password");

                var existingUser = UserManager.Users.FirstOrDefault(o => o.Email.EndsWith(domain));
                
                if (existingUser != null)
                    throw new InvalidOperationException("Already tenant admin user is created");

                var result = await UserManager.CreateAsync(new PhishingPortalUser
                {
                    UserName = user.Email,
                    Email = user.Email,
#if DEBUG
                    EmailConfirmed = true,
#endif 
                }, user.Password);

                // TODO: assign tenant_admin role
                
                if (!result.Succeeded)
                    throw new Exception("User creation failed. Please try again or contact support team");

                else
                {
                    var newUser = UserManager.Users.FirstOrDefault(o => o.Email == user.Email);
                    if (newUser != null)
                    {
                        var claims = new List<System.Security.Claims.Claim>()
                        {
                            new System.Security.Claims.Claim("role", "tenantadmin"),
                            new System.Security.Claims.Claim("tenant", tenant.UniqueId)
                        };

                        await UserManager.AddClaimsAsync(newUser, claims);
                       
                    }
                }

                response.IsSuccess = true;
                response.Message = "User created successfully";
                response.Result = true;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"Error create tenant's default user");
                return new ApiResponse<bool>() { Message = ex.Message };
            }

            return response;
        }



    }
}
