using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lykke.Service.ClientSearch.FullTextSearch
{
    public class PersonalDataLoader
    {
        private static DateTimeOffset lastTimeStamp = DateTimeOffset.MinValue;

        public static void LoadAllAsync(string connectionString, string tableName, ILog log)
        {
            log.WriteInfoAsync(nameof(PersonalDataLoader), "LoadAllAsync", "GET", "Starting");

            try
            {
                AzureTableStorage<PersonalDataEntity> repo = new AzureTableStorage<PersonalDataEntity>(connectionString, tableName, log);
                IList<PersonalDataEntity> allPersonalData = repo.GetDataAsync().GetAwaiter().GetResult();
                foreach(PersonalDataEntity e in allPersonalData)
                {
                    if (e.Timestamp > lastTimeStamp)
                    {
                        lastTimeStamp = e.Timestamp;
                    }
                }

                log.WriteInfoAsync(nameof(PersonalDataLoader), "LoadAllAsync", "GET", "Personal data loaded");

                Indexer.CreateIndex(allPersonalData);
                log.WriteInfoAsync(nameof(PersonalDataLoader), "LoadAllAsync", "GET", "Started");
            }
            catch (Exception ex)
            {
                log.WriteErrorAsync(nameof(PersonalDataLoader), "LoadAllAsync", "GET", ex);
            }
            finally
            {
                new Thread(() =>
                {
                    LoadNewRecords(connectionString, tableName, log);
                }
                ).Start();
            }
        }

        public static void LoadNewRecords(string connectionString, string tableName, ILog log)
        {
            for (;;)
            {
                try
                {
                    AzureTableStorage<PersonalDataEntity> repo = new AzureTableStorage<PersonalDataEntity>(connectionString, tableName, log);
                    IList<PersonalDataEntity> allPersonalData = repo.GetDataAsync(_ => _.Timestamp > lastTimeStamp).GetAwaiter().GetResult();
                    foreach (PersonalDataEntity e in allPersonalData)
                    {
                        if (e.Timestamp > lastTimeStamp)
                        {
                            lastTimeStamp = e.Timestamp;
                        }
                    }

                    Indexer.CreateIndex(allPersonalData);
                }
                catch
                {
                }
                finally
                {
                    Thread.Sleep(10000);
                }
            }
        }

    }
}
