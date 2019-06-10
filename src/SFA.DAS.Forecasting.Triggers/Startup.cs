using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
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
                .Build();

            Configuration = config;
        }

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();

            services.Configure<ForecastingJobs>(Configuration.GetSection("ForecastingJobs"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ForecastingJobs>>().Value);

            var serviceProvider = services.BuildServiceProvider();

            var config = serviceProvider.GetService<ForecastingJobs>();

            services.AddSingleton(_ =>
                _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));
            
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            return services.BuildServiceProvider();
        }
    }
}
