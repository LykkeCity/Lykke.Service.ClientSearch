using Lykke.JobTriggers.Triggers;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services
{
    public class IndexerManager
    {
        private TriggerHost _triggerHost;
        private Task _triggerHostTask;

        private readonly IServiceProvider _serviceProvider;
        private readonly Indexer _indexer;

        public IndexerManager(
            IServiceProvider serviceProvider,
            Indexer indexer
            )
        {
            _serviceProvider = serviceProvider;
            _indexer = indexer;
        }

        public async Task StartAsync()
        {
            await Task.Factory.StartNew(
                async () =>
                {
                    await _indexer.Initialize();

                    _triggerHost = new TriggerHost(_serviceProvider);
                    _triggerHostTask = _triggerHost.Start();
                }
            );
        }

        public async Task StopAsync()
        {
            _triggerHost?.Cancel();
            if (_triggerHostTask != null)
            {
                await _triggerHostTask;
            }
        }

    }
}
