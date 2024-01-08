using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace SFA.DAS.Forecasting.Commitments.Functions.AppStart;

public static class NServiceBusExtensions
{
    public static IServiceCollection ConfigureNServiceBus(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        if (!configuration.IsLocalOrDev())
        {
            services.AddNServiceBus(logger);
        }
        else
        {
            services.AddNServiceBus(logger, options =>
            {
                if (configuration["NServiceBusConnectionString"] == "UseLearningEndpoint=true")
                {
                    options.EndpointConfiguration = endpoint =>
                    {
                        endpoint
                            .UseTransport<LearningTransport>()
                            .StorageDirectory(configuration["NServiceBusStorageDirectory"]);
                        return endpoint;
                    };
                }
            });
        }
        
        return services;
    }
}