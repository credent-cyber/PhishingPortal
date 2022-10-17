using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Controllers.Api.Abstraction;

namespace PhishingPortal.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class RequestController : BaseApiController
    {

        private class OnboardingConfig
        {
            public string TestEmailRecipient { get; set; } = "malay.pandey@credentinfotech.com";
            public string EmailContent { get; set; } = "Confirmation Email Content ###CONFIRM_LINK###";

        }


        public RequestController(ILogger<RequestController> logger, IConfiguration config, ITenantAdminRepository tenantAdminRepo, INsLookupHelper nsLookup,
            IEmailSender emailSender,
            UserManager<PhishingPortalUser> userManager) : base(logger)
        {
            tenatAdminRepo = tenantAdminRepo;
            NsLookup = nsLookup;
            EmailSender = emailSender;
            UserManager = userManager;
            _config = new OnboardingConfig();
            config.GetSection("OnboardingConfig").Bind(_config);
        }

        OnboardingConfig _config;

        public ITenantAdminRepository tenatAdminRepo { get; }
        public INsLookupHelper NsLookup { get; }
        public IEmailSender EmailSender { get; }
        public UserManager<PhishingPortalUser> UserManager { get; }



        [HttpPost]
        [Route("upsert_demorequestor")]
        public async Task<DemoRequestor> UpsertDemoRequestor(DemoRequestor demoRequestor)
        {
            demoRequestor.CreatedOn = DateTime.Now;
            var demo = await tenatAdminRepo.UpsertDemoRequestor(demoRequestor);
            return demo;
        }
    }
}

