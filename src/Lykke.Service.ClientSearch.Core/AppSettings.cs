using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientSearch.Core
{
    public class AppSettings
    {
        //public AzureTableSettings LogConnection { get; set; }

        public PersonalDataApiSettings PersonalDataApi{ get; set; }

        public ClientSearchServiceSettings ClientSearchServiceSettings { get; set; }

        //public string ApiKey { get; set; }

    }


    /*
    public class ClientSearchSettings
    {
        public DbSettings Db { get; set; }
        public string ServiceUri { get; set; }
        public string ApiKey { get; set; }
    }

    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string PersonalDataConnString { get; set; }
    }
    */

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

    public class PersonalDataApiSettings
    {
        public AzureTableSettings PersonalDataConnection { get; set; }
    }

    public class ClientSearchServiceSettings
    {
        public AzureTableSettings LogConnection { get; set; }
    }


}
