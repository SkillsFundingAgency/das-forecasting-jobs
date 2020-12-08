using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Http;
using System;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(SFA.DAS.Forecasting.Commitments.Functions.Startup))]
namespace SFA.DAS.Forecasting.Commitments.Functions
{
    public class Startup : FunctionsStartup
    {
        private  ILoggerFactory _loggerFactory;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(logBuilder =>
            {
                logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information); // this is because all logging is filtered out by default
                var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
                logBuilder.AddNLog(Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0]);
            });

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>(); // Local

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

            var config = configBuilder.Build(); // azure storage explorer
            builder.Services.AddOptions();
            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            logger.LogDebug("Just before the Nservicebus :" + config["NServiceBusConnectionString"]);

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

            builder.Services.AddDbContext<ForecastingDbContext>(options =>
            options.UseSqlServer(config["DatabaseConnectionString"]));

            builder.Services.AddScoped<IForecastingDbContext, ForecastingDbContext>(provider => provider.GetService<ForecastingDbContext>());

            ConfigureLogFactoy();

            CommitmentsClientApiConfiguration commitmentsClientApiConfig = GetCommitmentsClientApiConfiguration(builder, serviceProvider, config);
            builder.Services.AddSingleton<ICommitmentsApiClientFactory>(x => new CommitmentsApiClientFactory(commitmentsClientApiConfig, _loggerFactory));
            builder.Services.AddTransient<ICommitmentsApiClient>(provider => provider.GetRequiredService<ICommitmentsApiClientFactory>().CreateClient());
                                  
            var mapperConfig = new MapperConfiguration(config => { config.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);
        }

        private bool ConfigurationIsLocalOrDev(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
        }

        private CommitmentsClientApiConfiguration GetCommitmentsClientApiConfiguration(IFunctionsHostBuilder builder, ServiceProvider serviceProvider, IConfigurationRoot config)
        {
            CommitmentsClientApiConfiguration commitmentsClientApiConfig;
            if (ConfigurationIsLocalOrDev(config))
            {
                commitmentsClientApiConfig = new CommitmentsClientApiConfiguration
                {
                    ApiBaseUrl = config["CommitmentsV2ApiBaseUrl"],
                    IdentifierUri = config["CommitmentsV2ApiIdentifierUri"],
                    ClientId = config["CommitmentsV2ApiClientId"],
                    ClientSecret = config["CommitmentsV2ApiClientSecret"],
                    Tenant = config["CommitmentsV2ApiTenant"]
                };
            }
            else
            {
                builder.Services.Configure<CommitmentsClientApiConfiguration>(config.GetSection("CommitmentsV2Api"));
                builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<CommitmentsClientApiConfiguration>>().Value);
                commitmentsClientApiConfig = serviceProvider.GetService<CommitmentsClientApiConfiguration>();
            }

            return commitmentsClientApiConfig;
        }

        public void ConfigureLogFactoy()
        {            
            _loggerFactory = new LoggerFactory();
            var logger = _loggerFactory.CreateLogger("Startup");            
        }       

    }   
}
