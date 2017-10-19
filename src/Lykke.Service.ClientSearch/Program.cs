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
        public static void Main(string[] args)
        {
            var webHostCancellationTokenSource = new CancellationTokenSource();
            var end = new ManualResetEvent(false);
            TriggerHost triggerHost = null;
            Task triggerHostTask = null;
            Task webHostTask = null;

            try
            {
                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");

                    webHostCancellationTokenSource.Cancel();

                    end.WaitOne();
                };

                /*
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseUrls("http://*:5048")
                    .UseApplicationInsights()
                    .Build();
                    */

                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls("http://*:5048")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();


                triggerHost = new TriggerHost(host.Services);

                webHostTask = Task.Factory.StartNew(() => host.RunAsync(webHostCancellationTokenSource.Token));

                triggerHostTask = triggerHost.Start();

                // WhenAny to handle any task termination with exception, 
                // or gracefully termination of webHostTask
                Task.WhenAny(webHostTask, triggerHostTask).Wait();

            }
            finally
            {
                Console.WriteLine("Terminating...");

                webHostCancellationTokenSource.Cancel();
                triggerHost?.Cancel();

                webHostTask?.Wait();
                triggerHostTask?.Wait();

                end.Set();

                Console.WriteLine("Terminated");
            }
        }
    }
}
