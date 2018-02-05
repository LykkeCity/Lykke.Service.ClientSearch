using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Lykke.JobTriggers.Triggers;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch
{
    internal static class Program
    {
        internal static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");

        static CancellationTokenSource webHostCancellationTokenSource = new CancellationTokenSource();
        static IWebHost webHost = null;
        static TriggerHost triggerHost = null;
        static Task webHostTask = null;
        static Task triggerHostTask = null;
        static ManualResetEvent end = new ManualResetEvent(false);

        public static void Start()
        {
            triggerHost = new TriggerHost(webHost.Services);
            triggerHostTask = triggerHost.Start();
        }

        static void Main(string[] args)
        {
            try
            {
                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");
                    webHostCancellationTokenSource.Cancel();
                    end.WaitOne();
                };

                webHost = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls("http://*:5048")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                webHostTask = webHost.RunAsync(webHostCancellationTokenSource.Token);
                webHostTask.Wait();
            }
            finally
            {
                Console.WriteLine("Terminating...");

                webHostCancellationTokenSource.Cancel();
                triggerHost?.Cancel();

                webHostTask?.Wait();
                triggerHostTask?.Wait();

                end.Set();
            }
        }
    }
}
