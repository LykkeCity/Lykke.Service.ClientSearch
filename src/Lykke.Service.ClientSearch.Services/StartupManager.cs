﻿using Common.Log;
using Lykke.JobTriggers.Triggers;
using Lykke.Service.ClientSearch.Core.Services;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly IndexerManager _indexerManager;
        private readonly ILog _log;

        public StartupManager(
            IndexerManager indexerManager,
            ILog log
            )
        {
            _indexerManager = indexerManager;
            _log = log;
        }

        public async Task StartAsync()
        {
            // TODO: Implement your startup logic here. Good idea is to log every step

            _indexerManager.Start();
            await Task.CompletedTask;
        }
    }
}
