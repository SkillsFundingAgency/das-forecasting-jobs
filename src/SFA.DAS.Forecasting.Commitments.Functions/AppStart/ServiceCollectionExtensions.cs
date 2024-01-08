using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using System;
using System.IO;
using System.Reflection;
using NLog.Extensions.Logging;

namespace SFA.DAS.Forecasting.Commitments.Functions.AppStart;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNServiceBus(
        this IServiceCollection serviceCollection,
        ILogger logger,
        Action<NServiceBusOptions> OnConfigureOptions = null)
    {
        var webBuilder = serviceCollection.AddWebJobs(x => { });
        webBuilder.AddExecutionContextBinding();

        var options = new NServiceBusOptions
        {
            OnMessageReceived = (context) =>
            {
                context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                logger.LogInformation($"Received NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");

            },
            OnMessageErrored = (ex, context) =>
            {
                context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                logger.LogError(ex, $"Error handling NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");
            }
        };

        if (OnConfigureOptions != null)
        {
            OnConfigureOptions.Invoke(options);
        }

        webBuilder.AddExtension(new NServiceBusExtensionConfigProvider(options));

        return serviceCollection;
    }

    public static IServiceCollection AddDasLogging(this IServiceCollection services)
    {
        services.AddLogging(logBuilder =>
        {
            logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information); // this is because all logging is filtered out by default
            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
            logBuilder.AddNLog(Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0]);
        });
        
        return services;
    }
}