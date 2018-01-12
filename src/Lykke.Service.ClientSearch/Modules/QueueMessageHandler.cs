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
        private readonly ILog _log;
        private readonly IPersonalDataService _personalDataService;

        public QueueMessageHandler(ILog log, IPersonalDataService personalDataService)
        {
            _log = log;
            _personalDataService = personalDataService;
        }

        [QueueTrigger("client-search-reindex-documents", 1000)]
        public async Task ProcessInMessage(ReindexRequest req)
        {
            try
            {
                IPersonalData docToIndex = await _personalDataService.GetAsync(req.ClientId);
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
