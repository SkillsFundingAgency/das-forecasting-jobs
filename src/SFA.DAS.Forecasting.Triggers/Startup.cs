using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Application.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.DependencyInjection;
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
                    o.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    o.EnvironmentName = configuration["EnvironmentName"];
                    o.ConfigurationKeys = configuration["ConfigNames"].Split(',');
                    o.PreFixConfigurationKeys = false;
                })
                .Build();

            Configuration = config;
        }

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();

            services.Configure<ForecastingJobsConfiguration>(Configuration.GetSection("forecastingJobsConfiguration"));

            services.AddSingleton(_ =>
                _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));

            services.AddSingleton<ILevyCompleteTriggerHandler, LevyCompleteTriggerHandler>();
            services.AddSingleton(typeof(IHttpFunctionClient<>), typeof(HttpFunctionClient<>));
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            return services.BuildServiceProvider();
        }
    }
}
