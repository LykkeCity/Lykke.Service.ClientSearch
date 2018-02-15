using Common.Log;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public partial class Indexer
    {
        public bool IsIndexReady { get; set; } = false;

        public async Task Initialize()
        {
            var retryPolicy =
                Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(
                    retryAttempt => TimeSpan.FromMinutes(GetRetryDelayByRetryAttempt(retryAttempt)),
                    (exception, timespan) => _log.WriteErrorAsync(nameof(Indexer), nameof(Initialize), exception)
                );

            await retryPolicy.ExecuteAsync(async () =>
            {
                await LoadDataAndCreateIndexAsync();
            });
        }

        private double GetRetryDelayByRetryAttempt(int retryAttempt)
        {
            if (retryAttempt < 3)
            {
                return 1;
            }
            else if (retryAttempt < 5)
            {
                return 5;
            }
            else
            {
                return 60;
            }
        }

        private async Task LoadDataAndCreateIndexAsync()
        {
            IEnumerable<IPersonalData> allPersonalData = await LoadPersonalDataAsync();

            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadDataAndCreateIndexAsync), "Index creation started");
            CreateIndex(allPersonalData, null);
            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadDataAndCreateIndexAsync), "Index creation completed");

            IsIndexReady = true;

            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadDataAndCreateIndexAsync), "Azure queue triggers started");
        }

        private async Task<IEnumerable<IPersonalData>> LoadPersonalDataAsync()
        {
            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadPersonalDataAsync), "Personal data loading started");
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

            await _log.WriteInfoAsync(nameof(Indexer), nameof(LoadPersonalDataAsync), "Personal data loading completed");

            return allPersonalData;
        }
    }
}
