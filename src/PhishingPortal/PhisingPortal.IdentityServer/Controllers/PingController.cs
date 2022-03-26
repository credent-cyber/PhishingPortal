using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace PhishingPortal.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class PingController : ControllerBase
    {
        private readonly ILogger<PingController> _logger;

        public PingController(ILogger<PingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ServiceStatus Get()
        {
            return new ServiceStatus { Name = "PhishingPortal.IdentityServer" };
        }
    }
}
