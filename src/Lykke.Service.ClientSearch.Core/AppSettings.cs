using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientSearch.Core
{
    public class AppSettings
    {
        //public PersonalDataServiceSettings PersonalDataServiceSettings { get; set; }

        public ClientSearchServiceSettings ClientSearchService { get; set; }

    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }

        public int ThrottlingLimitSeconds { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

    public class AzureTableSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string ConnectionString { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TableName { get; set; }
    }

    public class PersonalDataServiceSettings
    {
        public AzureTableSettings PersonalDataConnection { get; set; }
    }

    public class ClientSearchServiceSettings
    {
        public AzureTableSettings Log { get; set; }
        public string ClientPersonalInfoConnString { get; set; }
    }


}
