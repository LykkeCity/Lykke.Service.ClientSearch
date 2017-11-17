using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Lykke.JobTriggers.Triggers;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch
{
    public static class Program
    {
        static CancellationTokenSource webHostCancellationTokenSource = new CancellationTokenSource();
        static IWebHost webHost = null;
        static TriggerHost triggerHost = null;
        static Task webHostTask = null;
        static Task triggerHostTask = null;
        static ManualResetEvent end = new ManualResetEvent(false);

        public static void StartTriggers()
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

                // WhenAny to handle any task termination with exception, 
                // or gracefully termination of webHostTask
                //Task.WhenAny(webHostTask, triggerHostTask).Wait();
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
