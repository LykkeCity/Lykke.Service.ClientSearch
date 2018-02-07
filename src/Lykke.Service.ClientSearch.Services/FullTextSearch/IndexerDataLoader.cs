using Common.Log;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public partial class Indexer
    {
        public bool IndexCreated { get; set; } = false;

        public async Task LoadAllPersonalDataForIndexingAsync()
        {
            try
            {
                IEnumerable<IPersonalData> allPersonalData = await LoadPersonalDataAsync();
                await IndexPersonalDataAsync(allPersonalData);
                IndexCreated = true;

                _triggerManager.StartTriggers();
                await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadAllPersonalDataForIndexingAsync), "Azure queue triggers started");
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(Indexer), nameof(LoadAllPersonalDataForIndexingAsync), ex);
            }
        }

        private async Task IndexPersonalDataAsync(IEnumerable<IPersonalData> allPersonalData)
        {
            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadAllPersonalDataForIndexingAsync), "Index creation started");
            CreateIndex(allPersonalData, null);
            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadAllPersonalDataForIndexingAsync), "Index creation completed");
        }

        private async Task<IEnumerable<IPersonalData>> LoadPersonalDataAsync()
        {
            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadAllPersonalDataForIndexingAsync), "Personal data loading started");
            List<IPersonalData> allPersonalData = new List<IPersonalData>();

            string nextPage = null;
            int pageNum = 0;

            PagingInfoModel pim = new PagingInfoModel();
            pim.CurrentPage = 0;
            pim.ElementCount = 500;
            pim.NavigateToPageIndex = pageNum;
            pim.NextPage = nextPage;
            pim.PreviousPages = new List<string>();

            for (; ; )
            {
                pim.NavigateToPageIndex = pageNum;
                pim.NextPage = nextPage;

                var res = await _personalDataService.GetPagedAsync(pim);
                nextPage = res.PagingInfo.NextPage;

                pageNum++;
                if (res.Result != null)
                {
                    allPersonalData.AddRange(res.Result);
                }

                if (nextPage == null || res.Result == null || res.Result.Count() < pim.ElementCount)
                {
                    break;
                }
            }

            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadAllPersonalDataForIndexingAsync), "Personal data loading completed");

            return allPersonalData;
        }
    }
}
