using System;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Encoding;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Application.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.DependencyInjection;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Logging;
using SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus;
using SFA.DAS.Forecasting.Triggers;

[assembly: WebJobsStartup(typeof(Startup))]
namespace SFA.DAS.Forecasting.Triggers
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExecutionContextBinding();
            builder.AddDependencyInjection<ServiceProviderBuilder>();
            builder.AddExtension<NServiceBusExtensionConfig>();
        }
    }

    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        public IConfiguration Configuration { get; }
        public ServiceProviderBuilder(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .AddAzureTableStorage(o =>
                {
                    var configKeys = configuration["ConfigNames"]
                    .Split(',')
                    .Select(s => s.Trim())
                    .ToArray();

                    o.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    o.EnvironmentName = configuration["EnvironmentName"];
                    o.ConfigurationKeys = configKeys;
                    o.PreFixConfigurationKeys = false;
                })
                .Build();

            Configuration = config;
        }

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();

            services.Configure<ForecastingJobsConfiguration>(Configuration.GetSection("ForecastingJobs"));

            var nLogConfiguration = new NLogConfiguration();

            services.AddLogging((options) =>
            {
                options.AddConfiguration(Configuration.GetSection("Logging"));
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });

                options.AddConsole();
                options.AddDebug();

                nLogConfiguration.ConfigureNLog(Configuration);
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            services.AddSingleton(_ => _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));
            EncodingConfig encodingConfig = GetEncodingConfig();

            services.AddSingleton(encodingConfig);
            services.AddSingleton<IEncodingService, EncodingService>();

            services.AddSingleton<ILevyCompleteTriggerHandler, LevyCompleteTriggerHandler>();
            services.AddSingleton<IRefreshPaymentDataCompletedTriggerHandler, PaymentCompleteTriggerHandler>();
            services.AddSingleton(typeof(IHttpFunctionClient<>), typeof(HttpFunctionClient<>));
            //services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            return services.BuildServiceProvider();
        }

        private EncodingConfig GetEncodingConfig()
        {
            var encodingConfig = new EncodingConfig();
            encodingConfig.Encodings = Configuration
                .GetSection("Encodings")
                .GetChildren()
                .Select(encConfig =>
                {
                    var a = Enum.Parse(typeof(EncodingType), encConfig["EncodingType"]);
                    return new Encoding.Encoding
                    {
                        EncodingType = (EncodingType)Enum.Parse(typeof(EncodingType), encConfig["EncodingType"]),
                        Alphabet = encConfig["Alphabet"],
                        MinHashLength = int.Parse(encConfig["MinHashLength"]),
                        Salt = encConfig["Salt"],
                    };
                }).ToList();
            return encodingConfig;
        }
    }
}
