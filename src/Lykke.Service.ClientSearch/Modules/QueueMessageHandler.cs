using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.ClientSearch.Models;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Modules
{
    class QueueMessageHandler
    {
        private readonly Indexer _indexer;
        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;

        public QueueMessageHandler(
            Indexer indexer,
            IPersonalDataService personalDataService,
            ILog log
            )
        {
            _indexer = indexer;
            _personalDataService = personalDataService;
            _log = log;
        }

        [QueueTrigger("client-search-reindex-documents", 1000)]
        public async Task ProcessInMessageAsync(ReindexRequest req)
        {
            try
            {
                IPersonalData docToIndex = await _personalDataService.GetAsync(req.ClientId);
                if (docToIndex != null)
                {
                    await _indexer.IndexSingleDocumentAsync(req.ClientId, docToIndex);
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("QueueMessageHandler", "ProcessInMessage", req.ToJson(), ex);
            }

        }
    }


}
