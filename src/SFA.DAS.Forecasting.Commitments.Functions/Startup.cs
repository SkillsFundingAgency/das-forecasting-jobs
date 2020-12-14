using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Configuration.AzureTableStorage;
using System;
using System.IO;
using System.Reflection;
using SFA.DAS.Forecasting.Commitments.Functions.AppStart;

[assembly: FunctionsStartup(typeof(SFA.DAS.Forecasting.Commitments.Functions.Startup))]
namespace SFA.DAS.Forecasting.Commitments.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(logBuilder =>
            {
                logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information); // this is because all logging is filtered out by default
                var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
                logBuilder.AddNLog(Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0]);
            });

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var environment = configuration["EnvironmentName"];

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!ConfigurationIsLocalOrDev(environment))
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
            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            if (!ConfigurationIsLocalOrDev(environment))
            {
                builder.Services.AddNServiceBus(logger);
            }
            else
            {
                builder.Services.AddNServiceBus(
                    logger,
                    (options) =>
                    {
                        if (config["NServiceBusConnectionString"] == "UseLearningEndpoint=true")
                        {
                            options.EndpointConfiguration = (endpoint) =>
                            {
                                endpoint.UseTransport<LearningTransport>()
                                .StorageDirectory(config["NServiceBusStorageDirectory"]);
                                return endpoint;
                            };
                        }
                    });
            }

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddDatabaseRegistration(config, environment );
        }

        private bool ConfigurationIsLocalOrDev(string environment)
        {
            return environment.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   environment.Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
