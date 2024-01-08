using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.Forecasting.Commitments.Functions;
using SFA.DAS.Forecasting.Commitments.Functions.AppStart;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers.Services;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.Forecasting.Commitments.Functions;

public class Startup : FunctionsStartup
{
    private readonly ILoggerFactory _loggerFactory = new LoggerFactory();

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddDasLogging();

        var serviceProvider = builder.Services.BuildServiceProvider();
        
        var configuration = serviceProvider
            .GetService<IConfiguration>()
            .BuildDasConfiguration();
        
        var environment = configuration["EnvironmentName"];

        builder.Services.AddSingleton<IConfiguration>(configuration);
        
        var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

        builder.Services.ConfigureNServiceBus(configuration, logger);
        
        var mapperConfig = new MapperConfiguration(mapperConfigurationExpression =>
        {
            mapperConfigurationExpression.AddProfile<AutoMapperProfile>();
        });
        
        var mapper = mapperConfig.CreateMapper();
        builder.Services.AddSingleton(mapper);

        builder.Services.AddScoped(_ => configuration.CreateDocumentSession());

        var commitmentsClientApiConfig = configuration.GetCommitmentsClientApiConfiguration(builder);
        builder.Services.AddSingleton<ICommitmentsApiClientFactory>(x => new CommitmentsApiClientFactory(commitmentsClientApiConfig, _loggerFactory));
        builder.Services.AddTransient<ICommitmentsApiClient>(provider => provider.GetRequiredService<ICommitmentsApiClientFactory>().CreateClient());

        builder.Services.AddScoped<IApprenticeshipCompletedEventHandler, ApprenticeshipCompletedEventHandler>();
        builder.Services.AddScoped<IApprenticeshipStoppedEventHandler, ApprenticeshipStoppedEventHandler>();
        builder.Services.AddScoped<IApprenticeshipStopDateChangedEventHandler, ApprenticeshipStopDateChangedEventHandler>();
        builder.Services.AddScoped<IApprenticeshipCompletionDateUpdatedEventHandler, ApprenticeshipCompletionDateUpdatedEventHandler>();

        builder.Services.AddScoped<IGetApprenticeshipService, GetApprenticeshipService>();

        builder.Services.AddDatabaseRegistration(configuration, environment);
    }
}