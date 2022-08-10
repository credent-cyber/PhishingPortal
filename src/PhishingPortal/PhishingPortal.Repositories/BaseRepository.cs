using Microsoft.Extensions.Logging;

namespace PhishingPortal.Repositories
{
    public class BaseRepository
    {
        public BaseRepository(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }
    }
} 