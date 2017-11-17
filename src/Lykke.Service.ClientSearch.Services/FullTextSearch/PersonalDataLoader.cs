using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Lykke.Service.ClientSearch.FullTextSearch
{
    public class PersonalDataLoader
    {
        private static DateTimeOffset lastTimeStamp = DateTimeOffset.MinValue;
        //private static string[] lastLoadedEntites = null;

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

            /*
            finally
            {
                new Thread(() =>
                {
                    LoadNewRecords(connectionString, tableName, log);
                }
                ).Start();
            }
            */
        }

        /*
        public static async void LoadNewRecords(string connectionString, string tableName, ILog log)
        {
            for (;;)
            {
                try
                {
                    AzureTableStorage<PersonalDataEntity> repo = new AzureTableStorage<PersonalDataEntity>(connectionString, tableName, log);
                    // IList<PersonalDataEntity> allPersonalData = repo.GetDataAsync(_ => _.Timestamp > lastTimeStamp).GetAwaiter().GetResult();

                    string dt = lastTimeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    var query = new TableQuery<PersonalDataEntity>
                    {
                        FilterString = $"Timestamp ge datetime'{dt}'"
                    };
                    IEnumerable<PersonalDataEntity> allPersonalData = await repo.WhereAsync(query);
                    if (allPersonalData != null && allPersonalData.Count() > 0)
                    {
                        foreach (PersonalDataEntity e in allPersonalData)
                        {
                            if (e.Timestamp > lastTimeStamp)
                            {
                                lastTimeStamp = e.Timestamp;
                            }
                        }

                        if (lastLoadedEntites == null)
                        {
                            lastLoadedEntites = allPersonalData.OrderBy(_ => _.Timestamp).Select(_ => _.Id + _.Timestamp.Ticks).ToArray();
                        }
                        else
                        {
                            if (lastLoadedEntites.SequenceEqual(allPersonalData.OrderBy(_ => _.Timestamp).Select(_ => _.Id + _.Timestamp.Ticks)))
                            {
                                lastTimeStamp = lastTimeStamp.AddSeconds(1);
                            }
                        }

                        Indexer.CreateIndex(allPersonalData);
                    }
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
        */

    }
}
