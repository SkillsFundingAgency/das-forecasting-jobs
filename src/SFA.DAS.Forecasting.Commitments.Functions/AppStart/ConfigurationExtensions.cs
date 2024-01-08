using System;
using System.IO;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Forecasting.Jobs.Infrastructure.CosmosDB;

namespace SFA.DAS.Forecasting.Commitments.Functions.AppStart;

public static class ConfigurationExtensions
{
    public static IConfiguration BuildDasConfiguration(this IConfiguration configuration)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();

        if (!configuration.IsLocalOrDev())
        {
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }

        return configBuilder.Build();
    }
    
    public static CommitmentsClientApiConfiguration GetCommitmentsClientApiConfiguration(this IConfiguration configuration, IFunctionsHostBuilder builder)
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
            builder.Services.Configure<CommitmentsClientApiConfiguration>(section);
            builder.Services.AddSingleton(cfg => commitmentsClientApiConfig);
        }

        return commitmentsClientApiConfig;
    }
    
    public static bool IsLocalOrDev(this IConfiguration configuration)
    {
        var environment = configuration["EnvironmentName"];
        
        return environment.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
               environment.Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
    }
    
    public static IDocumentSession CreateDocumentSession(this IConfiguration config)
    {
        var connectionString = config["CosmosDbConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("No 'DocumentConnectionString' connection string found.");
        }

        var documentConnectionString = new DocumentSessionConnectionString(connectionString);

        var client = new DocumentClient(new Uri(documentConnectionString.AccountEndpoint),
            documentConnectionString.AccountKey);
        client.CreateDatabaseIfNotExistsAsync(new Database { Id = documentConnectionString.Database }).Wait();

        client.CreateDocumentCollectionIfNotExistsAsync(
            UriFactory.CreateDatabaseUri(documentConnectionString.Database), new DocumentCollection
            {
                Id = documentConnectionString.Collection
            },
            new RequestOptions { OfferThroughput = int.Parse(documentConnectionString.ThroughputOffer) }).Wait();

        return new DocumentSession(client, documentConnectionString);
    }
}