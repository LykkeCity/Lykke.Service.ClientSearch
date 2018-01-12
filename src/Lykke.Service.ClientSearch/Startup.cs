using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Service.ClientSearch.Core;
using Lykke.Service.ClientSearch.Modules;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Lykke.JobTriggers.Extenstions;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Client;
using System.IO;
using Microsoft.AspNetCore.Http;
using Lykke.Service.ClientSearch.Services.FullTextSearch;

namespace Lykke.Service.ClientSearch
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(x =>
            {
                if (!Environment.IsDevelopment())
                {
                    var authenticatedUserPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    x.Filters.Add(new AuthorizeFilter(authenticatedUserPolicy));
                }
            })
                .AddJsonOptions(options =>
                 {
                     options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                 });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "ClientSearch API");
            });

            var builder = new ContainerBuilder();

            var appSettings = Configuration.LoadSettings<AppSettings>();

            var log = CreateLogWithSlack(services, appSettings);

            builder.RegisterModule(new ServiceModule(log));

            builder.RegisterInstance<IPersonalDataService>(new PersonalDataService(appSettings.CurrentValue.PersonalDataServiceClient, log));

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(appSettings.CurrentValue.ClientSearchService.ClientPersonalInfoConnString);
            });

            builder.Populate(services);
            ApplicationContainer = builder.Build();


            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime, IPersonalDataService personalDataService, ILog log)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use((context, next) =>
            {
                if (!PersonalDataLoader.indexCreated)
                {
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        context.Response.StatusCode = 503;
                        return context.Response.WriteAsync("Search index is not ready yet");
                    }
                }

                return next();
            });


            app.UseLykkeMiddleware("ClientSearch", ex => new { Message = "Technical problem" });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();


            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationContainer.Dispose();
            });

            Task task = Task.Factory.StartNew(() => {
                PersonalDataLoader.LoadAllPersonalDataForIndexing(personalDataService, log);
                Program.Start();
            });
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.ClientSearchService.ClientPersonalInfoConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "LykkeClientSearchServiceLog", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}
