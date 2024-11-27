using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Encoding;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;
using SFA.DAS.Forecasting.Jobs.Application.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Services;
using SFA.DAS.Forecasting.Triggers.StartupExtensions;

[assembly: NServiceBusTriggerFunction("SFA.DAS.Forecasting.Jobs")]

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder => builder.BuildDasConfiguration())
    .ConfigureNServiceBus()
    .ConfigureServices((context, services) =>
    {
        services.AddDasLogging();

        var configuration = context.Configuration;

        services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));
        services.AddOptions();

        services.Configure<ForecastingJobsConfiguration>(configuration.GetSection("ForecastingJobs"));

        var mapperConfig = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>());

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        var encodingConfig = configuration.GetEncodingConfig();
        
        services.AddSingleton(encodingConfig);
        services.AddSingleton<IEncodingService, EncodingService>();

        services.AddSingleton<ILevyCompleteTriggerHandler, LevyCompleteTriggerHandler>();
        services.AddSingleton<IRefreshPaymentDataCompletedTriggerHandler, PaymentCompleteTriggerHandler>();
        services.AddSingleton<ILevyForecastService, LevyForecastService>();
        services.AddSingleton<IPaymentForecastService, PaymentForecastService>();
        services.AddSingleton(typeof(IHttpFunctionClient<>), typeof(HttpFunctionClient<>));

    })
    .Build();

await host.RunAsync();