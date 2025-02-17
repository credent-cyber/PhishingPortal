﻿using Microsoft.AspNetCore.Authorization;
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
    using DocumentFormat.OpenXml.Office2010.Drawing;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Newtonsoft.Json;
    using PhishingPortal.Dto.Subscription;
    using PhishingPortal.Licensing;
    using PhishingPortal.Server.Controllers.Api.Abstraction;
    using PhishingPortal.Server.Services;
    using PhishingPortal.Server.Services.Interfaces;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnboardingController : BaseApiController
    {

        private class OnboardingConfig
        {
            public string TestEmailRecipient { get; set; } = "malay.pandey@credentinfotech.com";
            public string EmailContent { get; set; } = "Confirmation Email Content ###CONFIRM_LINK###";

        }


        public OnboardingController(ILogger<OnboardingController> logger, IConfiguration config, ITenantAdminRepository tenantAdminRepo, INsLookupHelper nsLookup, ILicenseProvider licenseProvider,
            IEmailSender emailSender,
            UserManager<PhishingPortalUser> userManager) : base(logger)
        {
            this.tenantAdminRepo = tenantAdminRepo;
            NsLookup = nsLookup;
            LicenseProvider = licenseProvider;
            EmailSender = emailSender;
            UserManager = userManager;
            _config = new OnboardingConfig();
            config.GetSection("OnboardingConfig").Bind(_config);
        }

        OnboardingConfig _config;

        public ITenantAdminRepository tenantAdminRepo { get; }
        public INsLookupHelper NsLookup { get; }
        public ILicenseProvider LicenseProvider { get; }
        public IEmailSender EmailSender { get; }
        public UserManager<PhishingPortalUser> UserManager { get; }

        /// <summary>
        /// Step1 of tenant onboarding
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Register")]
        public async Task<ApiResponse<Tenant>> Register(Tenant tenant)
        {
            var response = new ApiResponse<Tenant>();
            Tenant t;

            try
            {
                response = await tenantAdminRepo.CreateTenantAsync(tenant);

                if (tenant.RequireDomainVerification)
                    await SendConfirmationEmail(tenant);

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                return new ApiResponse<Tenant>() { IsSuccess = false, Message = ex.Message };
            }
            return response;
        }


        [Authorize]
        [HttpGet]
        [Route("GetCurrentSubscription")]
        public async Task<ApiResponse<SubscriptionInfo>> GetCurrentSubscription(string tenantIdentifier)
        {
            var tenant = await tenantAdminRepo.GetByUniqueId(tenantIdentifier);
            var tenantDbContext = TenantDbResolver.CreateTenantDbContext(tenant);

            var license = tenantDbContext.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.License);
            var publicKey = tenantDbContext.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.PublicKey);

            if (license == null || string.IsNullOrEmpty(license.Value)
                || publicKey == null || string.IsNullOrEmpty(publicKey.Value))
            {
                return await Task.FromResult(new ApiResponse<SubscriptionInfo>() { IsSuccess = false });
            }

            var subscriptionInfo = LicenseProvider.GetSubscriptionInfo(license.Value, publicKey.Value);

            return await Task.FromResult(new ApiResponse<SubscriptionInfo>()
            {
                IsSuccess = subscriptionInfo != null,
                Result = subscriptionInfo
            });
        }

        [Authorize]
        [Route("CreateLicense")]
        public async Task<ActionResult> CreateLicenseKey(SubscriptionInfo subscriptionInfo)
        {
            try
            {
                var tenant = await tenantAdminRepo.GetByUniqueId(subscriptionInfo.TenantIdentifier);
                var currentUser = User.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Email);

                if (currentUser == null)
                    throw new InvalidOperationException("No authorized");

                var license = LicenseProvider.Generate(subscriptionInfo.TenantIdentifier, subscriptionInfo);

                var licenseData = new List<TenantData>
                {
                    new TenantData
                    {
                        Key = TenantData.Keys.License,
                        Value = license.Content,
                        TenantId = tenant.Id
                    },
                    new TenantData
                    {
                        Key = TenantData.Keys.PublicKey,
                        Value = license.PublicKey,
                        TenantId = tenant.Id
                    },
                      new TenantData
                    {
                        Key = TenantData.Keys.PrivateKey,
                        Value = license.PrivateKey,
                        TenantId = tenant.Id
                    }
                };

                await tenantAdminRepo.UpsertLicenseInfo(licenseData, currentUser.Value ?? "admin");
                var tenantDbContext = TenantDbResolver.CreateTenantDbContext(tenant);
                var tenantSettings = new List<TenantSetting>()
                {
                    new TenantSetting
                    {
                        Key = TenantData.Keys.License,
                        Value = license.Content
                    },

                    new TenantSetting
                    {
                        Key = TenantData.Keys.PublicKey,
                        Value = license.PublicKey
                    }

                };

                await tenantAdminRepo.UpsertTenantDbLicenseInfo(tenantSettings, tenantDbContext, currentUser.Value ?? "admin");

                await tenantDbContext.SaveChangesAsync();

                return await Task.FromResult(new FileContentResult(license.Content.ToByteArray(), "text/plain"));
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"Error while creating license for tenant - {subscriptionInfo.TenantIdentifier}");
                throw;
            }
        }


        [HttpGet]
        public async Task<List<Tenant>> GetAll(int pageIndex, int pageSize)
        {
            return await tenantAdminRepo.GetAllAsync(pageIndex, pageSize);
        }

        [HttpGet]
        [Route("TenantByUniqueId")]
        public async Task<Tenant> GetByUniqueId(string uniqueId)
        {

            return await tenantAdminRepo.GetByUniqueId(uniqueId);
        }

        [HttpDelete]
        [Route("DeleteTenantByUniqueId")]
        public async Task<(bool, string)> DeleteTenantByUniqueId(string uniqueId)
        {

            return await tenantAdminRepo.DeleteTenantByUniqueId(uniqueId);
        }


        [HttpPost]
        [Route("Provision")]
        [Obsolete("This method is not used anymore")]
        public async Task Provision(ProvisionTenantRequest request)
        {
            var result = await tenantAdminRepo.ProvisionAsync(request.TenantId, request.ConnectionString);

            if (result == false)
                NotFound("Tenant not registered yet");
        }

        /// <summary>
        /// Step 2 - Confirmation link hit
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Confirm")]
        [AllowAnonymous]
        public async Task<ApiResponse<Tenant>> Confirm(TenantConfirmationRequest request)
        {
            var response = new ApiResponse<Tenant>();
            Tenant t;
            try
            {

                t = await tenantAdminRepo.ConfirmRegistrationAsync(request.UniqueId, request.RegisterationHash, request.Url);
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

        /// <summary>
        /// Step 3 - domain verification
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ConfirmDomain")]
        [AllowAnonymous]
        public async Task<ApiResponse<Tenant>> ConfirmDomain(DomainVerificationRequest domain)
        {
            var response = new ApiResponse<Tenant>();
            Tenant confirmedTenant;
            try
            {
                var tenant = await tenantAdminRepo.GetByUniqueId(domain.UniqueId);
                if (tenant == null)
                {
                    Logger.LogError($"Tenant with uniqueid: [{domain.UniqueId}] not found");
                    response.IsSuccess = true;
                    response.Message = "Sucessfully verified domain";
                    return response;
                }

                var verficationResult = false;
                if (tenant.RequireDomainVerification)
                {
                    verficationResult = NsLookup.VerifyDnsRecords("TXT", domain.Domain.Trim().ToLower(), domain.DomainVerificationCode);
                }
                else
                {
                    Logger.LogWarning($"The domain verfication has been by passed for this tenant with unique ID: {tenant.UniqueId}");
                    verficationResult = true;
                }
                // do actual domain verification
                if (verficationResult)
                {
                    confirmedTenant = await tenantAdminRepo.ConfirmDomainAsync(domain);
                    response.IsSuccess = true;
                    response.Message = "Sucessfully verified domain, Now create credential to log in the portal.";
                    response.Result = confirmedTenant;
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

        /// <summary>
        /// Step 4- Tenant admin user creation
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost]
        [Route("CreateDefaultUser")]
        [AllowAnonymous]
        public async Task<ApiResponse<bool>> CreateDefaultUser(TenantAdminUser user)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var domain = user.Email.Split("@")[1];
                var tenant = await tenantAdminRepo.GetByDomain(domain);

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
                    EmailConfirmed = !tenant.RequireDomainVerification, // if tenant required domain verification then email confirmation is required
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

        /// <summary>
        /// Send confirmation email to the newely onboarded tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private async Task SendConfirmationEmail(Tenant tenant)
        {

            if (string.IsNullOrEmpty(tenant.ConfirmationLink))
                throw new ArgumentNullException($"Onboarding confirmation link is empty");

            var to = tenant.ContactEmail;
            var subject = $"PhishSim: Onboarding Confirmation Tenant ({tenant.UniqueId})";
            var mailContent = "Hi there, <br /><p>";
            mailContent += "You have been onboarded as new PhishSim application tenant.</p>";
            mailContent += $"<p>Pleae confirm by clicking <a href='###CONFIRM_LINK###'>here</a> and proceed to the onboarding process</p>";
            mailContent += "<br /><br /> System Admin <br/> PhishSim @CredentInfotech.com";

            mailContent = mailContent.Replace("###CONFIRM_LINK###", tenant.ConfirmationLink);

#if DEBUG
            to = _config.TestEmailRecipient;
#endif
            await EmailSender.SendEmailAsync(to, subject, mailContent);
        }


    }
}
