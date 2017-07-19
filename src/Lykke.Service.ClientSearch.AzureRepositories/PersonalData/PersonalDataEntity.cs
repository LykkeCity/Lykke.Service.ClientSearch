using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.ClientSearch.AzureRepositories.PersonalData
{
    public class PersonalDataEntity : TableEntity
    {
        public string Id => RowKey;
        public string FullName { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}