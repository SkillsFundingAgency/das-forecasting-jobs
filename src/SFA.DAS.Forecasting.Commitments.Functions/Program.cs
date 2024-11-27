using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.Forecasting.Commitments.Functions.StartupExtensions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers.Services;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;

[assembly: NServiceBusTriggerFunction("SFA.DAS.Forecasting.Functions")]

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder => builder.BuildDasConfiguration())
    .ConfigureNServiceBus()
    .ConfigureServices((context, services) =>
    {
        services.AddDasLogging();

        var configuration = context.Configuration;

        var environment = configuration["EnvironmentName"];

        services.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var mapperConfig = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>());

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        services.AddScoped(_ => configuration.CreateDocumentSession());

        var commitmentsClientApiConfig = configuration.GetCommitmentsClientApiConfiguration(services);
        var loggingFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggingFactory.CreateLogger(typeof(Program));
        logger.LogInformation("Program startup CosmosDbConnectionString: {Value}", configuration["CosmosDbConnectionString"]);
        logger.LogInformation("Program startup CosmosDbReadOnlyConnectionString: {Value}", configuration["CosmosDbReadOnlyConnectionString"]);

        services.AddSingleton<ICommitmentsApiClientFactory>(x => new CommitmentsApiClientFactory(commitmentsClientApiConfig, loggingFactory));
        services.AddTransient<ICommitmentsApiClient>(provider => provider.GetRequiredService<ICommitmentsApiClientFactory>().CreateClient());

        services.AddScoped<IApprenticeshipCompletedEventHandler, ApprenticeshipCompletedEventHandler>();
        services.AddScoped<IApprenticeshipStoppedEventHandler, ApprenticeshipStoppedEventHandler>();
        services.AddScoped<IApprenticeshipStopDateChangedEventHandler, ApprenticeshipStopDateChangedEventHandler>();
        services.AddScoped<IApprenticeshipCompletionDateUpdatedEventHandler, ApprenticeshipCompletionDateUpdatedEventHandler>();

        services.AddScoped<IGetApprenticeshipService, GetApprenticeshipService>();

        services.AddDatabaseRegistration(configuration, environment);
    })
    .Build();

await host.RunAsync();