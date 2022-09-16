using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;

namespace PhishingPortal.Server.Controllers.Api
{
    //[Route("api/[controller]")]
    //[ApiController]
    [ODataRoutePrefix("api")]
    public class TemplateController : ODataController
    {
        private ILogger log;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TenantDbContext _context { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TemplateController(TenantDbContext context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _context = context;
        }
        //[HttpGet]
        //[EnableQuery]
        //public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        //{
        //    var repository = new TenantRepository(log, _context);
        //    var temp = repository.GetAllTemplates(pageIndex, pageSize);
        //    return Ok(temp); 
        //}

        [HttpGet]
        [EnableQuery]
        public IQueryable<CampaignTemplate> Get()
        {
            return _context.CampaignTemplates.AsQueryable();
        }
    }
}
