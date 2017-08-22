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
using Lykke.Service.ClientSearch.FullTextSearch;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using AzureStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Lykke.JobTriggers.Extenstions;

namespace Lykke.Service.ClientSearch
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }
        public IConfigurationRoot Configuration { get; }
        public static AppSettings settings { get; set; }

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
            /*
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });
            */

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
            /*
            var appSettings = Environment.IsDevelopment()
                ? Configuration.Get<AppSettings>()
                : HttpSettingsLoader.Load<AppSettings>(Configuration.GetValue<string>("SettingsUrl"));
                */
            //string settingsUrl = Configuration.GetValue<string>("SettingsUrl");
            var appSettings = HttpSettingsLoader.Load<AppSettings>();

            var log = CreateLogWithSlack(services, appSettings);

            builder.RegisterModule(new ServiceModule(log));

            builder.RegisterInstance<AppSettings>(appSettings).SingleInstance();

            //builder.RegisterType<ApiKeyValidator>().As<IApiKeyValidator>();

            builder.RegisterInstance<INoSQLTableStorage<PersonalDataEntity>>(new AzureTableStorage<PersonalDataEntity>(appSettings.PersonalDataApi.PersonalDataConnection.ConnectionString, "PersonalData", log));
            builder.RegisterType<PersonalDataRepository>().As<IPersonalDataRepository>();

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(appSettings.PersonalDataApi.PersonalDataConnection.ConnectionString);
            });


            builder.Populate(services);
            ApplicationContainer = builder.Build();


            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime, AppSettings settings, ILog log)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLykkeMiddleware("ClientSearch", ex => new {Message = "Technical problem"});

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();


            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationContainer.Dispose();
            });

            Task task = Task.Factory.StartNew(() => PersonalDataLoader.LoadAllAsync(settings.PersonalDataApi.PersonalDataConnection.ConnectionString, "PersonalData", log));
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, AppSettings settings)
        {
            LykkeLogToAzureStorage logToAzureStorage = null;

            var logToConsole = new LogToConsole();
            var logAggregate = new LogAggregate();

            logAggregate.AddLogger(logToConsole);

            var dbLogConnectionString = settings.ClientSearchServiceSettings.LogConnection.ConnectionString;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                logToAzureStorage = new LykkeLogToAzureStorage("Lykke.Service.ClientSearch", new AzureTableStorage<LogEntity>(
                    dbLogConnectionString, settings.ClientSearchServiceSettings.LogConnection.TableName, logToConsole));

                logAggregate.AddLogger(logToAzureStorage);
            }

            // Creating aggregate log, which logs to console and to azure storage, if last one specified
            var log = logAggregate.CreateLogger();

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            /*
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.SlackNotifications.AzureQueue.QueueName
            }, log);

            // Finally, setting slack notification for azure storage log, which will forward necessary message to slack service
            logToAzureStorage?.SetSlackNotification(slackService);
            */

            return log;
        }
    }
}
