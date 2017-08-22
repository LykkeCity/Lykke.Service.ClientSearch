using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using Lykke.Service.ClientSearch.FullTextSearch;
using Lykke.Service.ClientSearch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Modules
{
    class QueueMessageHandler
    {
        private readonly ILog _log;
        private readonly IPersonalDataRepository _personalDataRepository;

        public QueueMessageHandler(ILog log, IPersonalDataRepository personalDataRepository)
        {
            _log = log;
            _personalDataRepository = personalDataRepository;
        }

        [QueueTrigger("client-search-reindex-documents", 1000)]
        public async Task ProcessInMessage(ReindexRequest req)
        {
            try
            {
                PersonalDataEntity docToIndex = await _personalDataRepository.GetAsync(req.ClientId);
                if (docToIndex != null)
                {
                    Indexer.IndexSingleDocument(docToIndex);
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("QueueMessageHandler", "ProcessInMessage", req.ToJson(), ex);
            }

        }
    }


}
