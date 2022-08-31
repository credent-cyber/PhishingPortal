using Microsoft.Extensions.Logging;

namespace PhishingPortal.Repositories
{
    public abstract class BaseRepository
    {
        public BaseRepository(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }
    }
} 