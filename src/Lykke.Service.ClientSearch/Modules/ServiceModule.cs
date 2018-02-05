﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.JobTriggers.Extenstions;
using Lykke.Service.ClientSearch.Core;
using Lykke.Service.ClientSearch.Core.Services;
using Lykke.Service.ClientSearch.Services;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.ClientSearch.Modules
{
    internal class ServiceModule : Module
    {
        private readonly IReloadingManager<ClientSearchServiceSettings> _settings;
        private readonly IReloadingManager<PersonalDataServiceClientSettings> _pdClientSettings;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public ServiceModule(
            IReloadingManager<ClientSearchServiceSettings> settings,
            IReloadingManager<PersonalDataServiceClientSettings> pdClientSettings,
            ILog log)
        {
            _settings = settings;
            _pdClientSettings = pdClientSettings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterInstance<IPersonalDataService>(new PersonalDataService(_pdClientSettings.CurrentValue, _log));

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(_settings.CurrentValue.ClientPersonalInfoConnString);
            });

            builder.Populate(_services);
        }
    }
}