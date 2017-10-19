using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Lykke.JobTriggers.Triggers;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch
{
    public class Program
    {
        static void Main(string[] args)
        {
            var webHostCancellationTokenSource = new CancellationTokenSource();
            IWebHost webHost = null;
            TriggerHost triggerHost = null;
            Task webHostTask = null;
            Task triggerHostTask = null;
            var end = new ManualResetEvent(false);

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

                triggerHost = new TriggerHost(webHost.Services);

                webHostTask = webHost.RunAsync(webHostCancellationTokenSource.Token);
                webHostTask.Wait();
                //triggerHostTask = triggerHost.Start();

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
