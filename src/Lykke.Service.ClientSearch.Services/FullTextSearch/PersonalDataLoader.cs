using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using System;
using System.Collections.Generic;

namespace Lykke.Service.ClientSearch.FullTextSearch
{
    public class PersonalDataLoader
    {
        public static void LoadAllAsync(string connectionString, string tableName, ILog log)
        {
            try
            {
                AzureTableStorage<PersonalDataEntity> repo = new AzureTableStorage<PersonalDataEntity>(connectionString, tableName, log);
                IList<PersonalDataEntity> allPersonalData = repo.GetDataAsync().GetAwaiter().GetResult();

                Indexer.CreateIndex(allPersonalData);
            }
            catch (Exception ex)
            {
                log.WriteErrorAsync(nameof(PersonalDataLoader), "LoadAllAsync", "GET", ex);
            }
        }
    }
}
