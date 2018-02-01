using Lykke.Service.PersonalData.Settings;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientSearch.Core
{
    public class AppSettings
    {
        public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }

        public ClientSearchServiceSettings ClientSearchService { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }
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

    public class ClientSearchServiceSettings
    {
        public string ClientPersonalInfoConnString { get; set; }
        public string LogsConnString { get; set; }
    }


}
