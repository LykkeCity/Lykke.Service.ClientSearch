using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Service.ClientSearch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHostCancellationTokenSource = new CancellationTokenSource();
            var end = new ManualResetEvent(false);

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine("SIGTERM recieved");

                webHostCancellationTokenSource.Cancel();

                end.WaitOne();
            };

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:5048")
                .UseApplicationInsights()
                .Build();

            host.Run(webHostCancellationTokenSource.Token);

            end.Set();

            Console.WriteLine("Terminated");
        }
    }
}
