using Microsoft.AspNetCore.Mvc;

namespace PhishingPortal.Server.Controllers
{
    public class BaseApiController: ControllerBase
    {
        public BaseApiController(ILogger logger) : base()
        {
            Logger = logger;
        }

        public ILogger Logger { get; }
    }

}
