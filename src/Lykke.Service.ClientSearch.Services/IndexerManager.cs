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
        private readonly int _taskCancelTime = 5000;

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

        public void Start()
        {
            Task.Run(
                async () =>
                {
                    await _indexer.Initialize();

                    _triggerHost = new TriggerHost(_serviceProvider);
                    _triggerHostTask = _triggerHost.Start();
                }
            );
        }

        public void Stop()
        {
            _triggerHost?.Cancel();
            if (_triggerHostTask != null)
            {
                _triggerHostTask.Wait(_taskCancelTime);
            }
        }

    }
}
