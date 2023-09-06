using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Domain.Configuration;

namespace SFA.DAS.Forecasting.Trigger.TestConsole;

class Program
{
    static void Main(string[] args)
    {
        Run(args).Wait();
    }

    public static async Task Run(string[] args)
    {
        var host = new HostBuilder()
            .UseEnvironment("local")
            .ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile("host.json");
                configHost.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                configApp.SetBasePath(Directory.GetCurrentDirectory());
                configApp.AddJsonFile("appsettings.json", optional: true);
                configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json");
                configApp.AddCommandLine(args);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ForecastingJobsConfiguration>(hostContext.Configuration.GetSection("ForecastingJobsConfiguration"));
                services.AddSingleton(cfg => cfg.GetService<IOptions<ForecastingJobsConfiguration>>().Value);
                services.AddTransient<NServiceBusConsole>();
                services.AddHostedService<LifetimeEventsHostedService>();
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddConsole();
                configLogging.AddDebug();
            })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }
}