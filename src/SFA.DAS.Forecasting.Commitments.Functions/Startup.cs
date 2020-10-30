using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Configuration.AzureTableStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

[assembly: FunctionsStartup(typeof(SFA.DAS.Forecasting.Commitments.Functions.Startup))]
namespace SFA.DAS.Forecasting.Commitments.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(logBuilder =>
            {
                logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information); // this is because all logging is filtered out by defualt
                var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
                logBuilder.AddNLog(Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0]);
            });

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!ConfigurationIsLocalOrDev(configuration))
            {
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }

            var config = configBuilder.Build();

            builder.Services.AddOptions();
            //builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));

            //builder.Services.AddCommandService();

            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            if (!ConfigurationIsLocalOrDev(config))
            {
                builder.Services.AddNServiceBus(logger);
            }
            else if (ConfigurationIsLocalOrDev(config))
            {
                builder.Services.AddNServiceBus(
                    logger,
                    (options) =>
                    {
                       // if (config["ApplicationSettings:NServiceBusConnectionString"] == "UseLearningEndpoint=true")
                        //{
                        //    options.EndpointConfiguration = (endpoint) =>
                        //    {
                        //        endpoint.UseTransport<LearningTransport>()
                        //        .StorageDirectory("D:\\EFSA\\das-commitments\\src\\.learningtransport");
                        //        return endpoint;
                        //    };
                        //}
                    });
            }

            builder.Services.AddDbContext<ForecastingDbContext>(options =>
            options.UseSqlServer("sqlServerConnectionString"));

            builder.Services.AddScoped<IForecastingDbContext, ForecastingDbContext>(provider => provider.GetService<ForecastingDbContext>());
        }

        private bool ConfigurationIsLocalOrDev(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
