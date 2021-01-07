using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.CosmosDB
{
    public interface IDocumentSession
    {
        Task<T> Get<T>(string id) where T : class, IDocument;
        Task<Document> GetDocument(string id);
    }

    public class DocumentSession : IDocumentSession
    {
        private readonly IDocumentClient _client;
        private readonly DocumentCollection _documentCollection;
        private readonly string _databaseId;
        public DocumentSession(IDocumentClient client, DocumentSessionConnectionString connectionString)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _databaseId = connectionString.Database;
            _documentCollection = client
                .ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(connectionString.Database, connectionString.Collection))
                .Result.Resource;
        }

        public static string GenerateDocumentId<T>(string id) where T : class, IDocument => $"{typeof(T).Name}-{id.ToLower()}";

        public async Task<T> Get<T>(string id) where T : class, IDocument
        {
            try
            {
                var response = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _documentCollection.Id, GenerateDocumentId<T>(id)));
                var adapter = JsonConvert.DeserializeObject<DocumentAdapter<T>>(response.Resource.ToString());
                return adapter?.Document;
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        public async Task<Document> GetDocument(string id)
        {
            try
            {
                var response = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _documentCollection.Id, id));
                return response.Resource;
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }
    }

    public class DocumentAdapter<T> where T : class, IDocument
    {
        public T Document { get; set; }

        // ReSharper disable once InconsistentNaming
        public string id { get; set; }

        // ReSharper disable once InconsistentNaming
        public string type { get => Document.GetType().FullName; set { } }

        protected DocumentAdapter() { } //for serialization
        public DocumentAdapter(string id, T document)
        {
            this.id = id;
            Document = document;
        }
    }

    public interface IDocument
    {
        string Id { get; set; }
    }

    public class DocumentSessionConnectionString : DbConnectionStringBuilder
    {
        public DocumentSessionConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string Database { get => (string)this["Database"]; set => this["Database"] = value; }
        public string AccountEndpoint { get => (string)this["AccountEndpoint"]; set => this["AccountEndpoint"] = value; }
        public string AccountKey { get => (string)this["AccountKey"]; set => this["AccountKey"] = value; }
        public string Collection { get => (string)this["Collection"]; set => this["Collection"] = value; }
        public string ThroughputOffer { get => (string)this["ThroughputOffer"]; set => this["ThroughputOffer"] = value; }
    }
}
