using AzureStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.AzureRepositories.PersonalData
{
    public class PersonalDataRepository : IPersonalDataRepository
    {
        private readonly INoSQLTableStorage<PersonalDataEntity> _tableStorage;

        public PersonalDataRepository(INoSQLTableStorage<PersonalDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<PersonalDataEntity> GetAsync(string id)
        {
            var partitionKey = GeneratePartitionKey();
            var rowKey = GenerateRowKey(id);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public static string GeneratePartitionKey()
        {
            return "PD";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }



    }
}
