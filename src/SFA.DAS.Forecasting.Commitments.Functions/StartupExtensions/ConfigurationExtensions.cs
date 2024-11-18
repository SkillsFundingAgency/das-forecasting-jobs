using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Forecasting.Commitments.Functions.StartupExtensions;

public static class ConfigurationExtensions
{
    public static IConfiguration BuildDasConfiguration(this IConfigurationBuilder configBuilder)
    {
        configBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();

        configBuilder.AddJsonFile("local.settings.json", optional: true);

        var config = configBuilder.Build();

        configBuilder.AddAzureTableStorage(options =>
        {
#if DEBUG
            options.ConfigurationKeys = config["Values:ConfigNames"].Split(",");
            options.StorageConnectionString = config["Values:ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["Values:EnvironmentName"];
#else
            options.ConfigurationKeys = config["ConfigNames"].Split(",");
            options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["EnvironmentName"];
#endif

            options.PreFixConfigurationKeys = false;
        });

        return configBuilder.Build();
    }
    
    public static bool IsLocalOrDev(this IConfiguration configuration)
    {
        var environment = configuration["EnvironmentName"];
        
        return environment.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
               environment.Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
    }
    
    public static CommitmentsClientApiConfiguration GetCommitmentsClientApiConfiguration(this IConfiguration configuration, IServiceCollection services)
    {
        CommitmentsClientApiConfiguration commitmentsClientApiConfig;
        
        if (configuration.IsLocalOrDev())
        {
            commitmentsClientApiConfig = new CommitmentsClientApiConfiguration
            {
                ApiBaseUrl = configuration["CommitmentsV2ApiBaseUrl"],
                IdentifierUri = configuration["CommitmentsV2ApiIdentifierUri"],
                ClientId = configuration["CommitmentsV2ApiClientId"],
                ClientSecret = configuration["CommitmentsV2ApiClientSecret"],
                Tenant = configuration["CommitmentsV2ApiTenant"]
            };
        }
        else
        {
            var section = configuration.GetSection("CommitmentsV2Api");
            commitmentsClientApiConfig = section.Get<CommitmentsClientApiConfiguration>();
            services.Configure<CommitmentsClientApiConfiguration>(section);
            services.AddSingleton(cfg => commitmentsClientApiConfig);
        }

        return commitmentsClientApiConfig;
    }
}