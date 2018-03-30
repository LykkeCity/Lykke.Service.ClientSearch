using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ClientSearch.Core.Services;

namespace Lykke.Service.ClientSearch.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.
    
    public class ShutdownManager : IShutdownManager
    {
        private readonly IndexerManager _indexerManager;
        private readonly ILog _log;

        public ShutdownManager(
            IndexerManager indexerManager,
            ILog log
            )
        {
            _indexerManager = indexerManager;
            _log = log;
        }

        public async Task StopAsync()
        {
            // TODO: Implement your shutdown logic here. Good idea is to log every step

            _indexerManager.Stop();
            await Task.CompletedTask;
        }
    }
}
