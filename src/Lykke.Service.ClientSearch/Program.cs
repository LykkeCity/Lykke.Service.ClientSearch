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
        internal static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");

        static CancellationTokenSource webHostCancellationTokenSource = new CancellationTokenSource();
        static IWebHost webHost = null;
        static Task webHostTask = null;
        static ManualResetEvent end = new ManualResetEvent(false);

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
                webHostTask?.Wait();

                end.Set();
            }
        }
    }
}
