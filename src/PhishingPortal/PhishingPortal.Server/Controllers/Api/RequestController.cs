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

        public RequestController(ILogger<RequestController> logger, IConfiguration config, ITenantAdminRepository tenantAdminRepo) : base(logger)
        {
            tenatAdminRepo = tenantAdminRepo;
        }

        public ITenantAdminRepository tenatAdminRepo { get; }

        [HttpPost]
        [Route("upsert_demorequestor")]
        public async Task<string> UpsertDemoRequestor(DemoRequestor demoRequestor)
        {
            demoRequestor.CreatedOn = DateTime.Now;
            var demo = await tenatAdminRepo.UpsertDemoRequestor(demoRequestor);
            return demo;
        }
    }
}

