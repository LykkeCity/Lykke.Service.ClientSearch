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
    public static class PersonalDataLoader
    {
        public static volatile bool indexCreated = false;
        private static DateTimeOffset lastTimeStamp = DateTimeOffset.MinValue;

        public static async Task LoadAllPersonalDataForIndexing(IPersonalDataService personalDataService, ILog log)
        {
            try
            {
                await log.WriteInfoAsync(nameof(PersonalDataLoader), nameof(LoadAllPersonalDataForIndexing), "Personal data loading started");
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

                    var res = personalDataService.GetPagedAsync(pim).GetAwaiter().GetResult();
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

                await log.WriteInfoAsync(nameof(PersonalDataLoader), nameof(LoadAllPersonalDataForIndexing), "Personal data loading completed");

                await log.WriteInfoAsync(nameof(PersonalDataLoader), nameof(LoadAllPersonalDataForIndexing), "Index creation started");
                Indexer.CreateIndex(allPersonalData, null);
                await log.WriteInfoAsync(nameof(PersonalDataLoader), nameof(LoadAllPersonalDataForIndexing), "Index creation completed");

                indexCreated = true;
            }
            catch (Exception ex)
            {
                await log.WriteErrorAsync(nameof(PersonalDataLoader), nameof(LoadAllPersonalDataForIndexing), ex);
            }
        }
    }
}
