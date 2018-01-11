using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.ClientSearch.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.ClientSearch.Modules
{
    public class ServiceModule : Module
    {
        //private readonly ClientSearchSettings _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(/*ClientSearchSettings settings, */ILog log)
        {
            //_settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            // TODO: Add your dependencies here

            builder.Populate(_services);
        }
    }
}
