using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Forecasting.Jobs.Infrastructure.CosmosDB;

namespace SFA.DAS.Forecasting.Commitments.Functions.StartupExtensions;

public static class DocumentSessionExtensions
{
    private const string ConnectionStringName = "CosmosDbConnectionString";

    public static IDocumentSession CreateDocumentSession(this IConfiguration config)
    {
        var connectionString = config[ConnectionStringName];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"No '{ConnectionStringName}' connection string found.");
        }

        var documentConnectionString = new DocumentSessionConnectionString(connectionString);

        var client = new DocumentClient(new Uri(documentConnectionString.AccountEndpoint), documentConnectionString.AccountKey);
        
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