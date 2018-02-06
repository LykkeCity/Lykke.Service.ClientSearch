using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ClientSearch.Core.Services;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Service.ClientSearch.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;

        public StartupManager(
            IPersonalDataService personalDataService,
            ILog log)
        {
            _personalDataService = personalDataService;
            _log = log;
        }

        public async Task StartAsync()
        {
            // TODO: Implement your startup logic here. Good idea is to log every step

            Task task = Task.Factory.StartNew(async () => {
                await PersonalDataLoader.LoadAllPersonalDataForIndexing(_personalDataService, _log);
            });
            await Task.CompletedTask;
        }
    }
}