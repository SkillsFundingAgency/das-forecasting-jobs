using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;
using SFA.DAS.Http;
using System;
using System.IO;
using System.Reflection;
using SFA.DAS.Forecasting.Commitments.Functions.AppStart;
using SFA.DAS.Forecasting.Jobs.Infrastructure.CosmosDB;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers.Services;

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

            ConfigureLogFactoy();

            CommitmentsClientApiConfiguration commitmentsClientApiConfig = GetCommitmentsClientApiConfiguration(builder, serviceProvider, config, environment);            
            builder.Services.AddSingleton<ICommitmentsApiClientFactory>(x => new CommitmentsApiClientFactory(commitmentsClientApiConfig, _loggerFactory));
            builder.Services.AddTransient<ICommitmentsApiClient>(provider => provider.GetRequiredService<ICommitmentsApiClientFactory>().CreateClient());
                                  
            var mapperConfig = new MapperConfiguration(config => { config.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);

            builder.Services.AddScoped<IApprenticeshipCompletedEventHandler, ApprenticeshipCompletedEventHandler>();
            builder.Services.AddScoped<IApprenticeshipStoppedEventHandler, ApprenticeshipStoppedEventHandler>();
            builder.Services.AddScoped<IApprenticeshipStopDateChangedEventHandler, ApprenticeshipStopDateChangedEventHandler>();
            builder.Services.AddScoped<IApprenticeshipCompletionDateUpdatedEventHandler, ApprenticeshipCompletionDateUpdatedEventHandler>();
            builder.Services.AddScoped(_ => CreateDocumentSession(config));
            builder.Services.AddScoped<IGetApprenticeshipService, GetApprenticeshipService>();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddDatabaseRegistration(config, environment );
            
        }

        private bool ConfigurationIsLocalOrDev(string environment)
        {
            return environment.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   environment.Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
        }

        private CommitmentsClientApiConfiguration GetCommitmentsClientApiConfiguration(IFunctionsHostBuilder builder, ServiceProvider serviceProvider, IConfigurationRoot config, string environment)
        {
            CommitmentsClientApiConfiguration commitmentsClientApiConfig;
            if (ConfigurationIsLocalOrDev(environment))
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
                var section = config.GetSection("CommitmentsV2Api");
                commitmentsClientApiConfig =  section.Get<CommitmentsClientApiConfiguration>();
                builder.Services.Configure<CommitmentsClientApiConfiguration>(section);
                builder.Services.AddSingleton(cfg => commitmentsClientApiConfig);
            }

            return commitmentsClientApiConfig;
        }

        public void ConfigureLogFactoy()
        {            
            _loggerFactory = new LoggerFactory();
            var logger = _loggerFactory.CreateLogger("Startup");            
        }

        protected IDocumentSession CreateDocumentSession(IConfigurationRoot config)
        {
            var connectionString = config["CosmosDbConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("No 'DocumentConnectionString' connection string found.");
            var documentConnectionString = new DocumentSessionConnectionString(connectionString);

            var client = new DocumentClient(new Uri(documentConnectionString.AccountEndpoint), documentConnectionString.AccountKey);
            client.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database { Id = documentConnectionString.Database }).Wait();

            client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(documentConnectionString.Database), new DocumentCollection
                {
                    Id = documentConnectionString.Collection
                },
                new RequestOptions { OfferThroughput = int.Parse(documentConnectionString.ThroughputOffer) }).Wait();
            return new DocumentSession(client, documentConnectionString);
        }

    }   
}
