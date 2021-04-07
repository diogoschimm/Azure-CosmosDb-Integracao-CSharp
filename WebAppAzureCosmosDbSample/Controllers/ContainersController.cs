using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppAzureCosmosDbSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainersController : BaseApi
    {
        private readonly string DatabaseName = "DbFornecedores";

        public ContainersController(IDocumentClient client) : base(client) { }

        [HttpGet]
        public async Task<IEnumerable<dynamic>> Get()
        {
            var dataBaseUri = UriFactory.CreateDatabaseUri(DatabaseName);

            IEnumerable<DocumentCollection> documentsCollections = await _client.ReadDocumentCollectionFeedAsync(dataBaseUri);
            return documentsCollections.ToList().Select(p => p.Id);
        }

        [HttpGet("{collectionName}")]
        public async Task<string> Get(string collectionName)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);

            DocumentCollection documentCollection = await _client.ReadDocumentCollectionAsync(collectionUri);
            return documentCollection.Id;
        }

        [HttpPost]
        public async Task<string> Post([FromBody] string documentCollection)
        {
            DocumentCollection myCollection = await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DatabaseName),
                new DocumentCollection { Id = documentCollection },
                new RequestOptions { OfferThroughput = 400 }
                );

            return myCollection.Id;
        }

        [HttpPut("{documentCollectionOld}")]
        public async Task<string> Put(string documentCollectionOld, [FromBody] string newDocumentCollection)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, documentCollectionOld);

            DocumentCollection myCollection = await _client.ReplaceDocumentCollectionAsync(
                  collectionUri, new DocumentCollection { Id = newDocumentCollection },
                  new RequestOptions { OfferThroughput = 400 }
                  );

            return myCollection.Id;
        }

        [HttpDelete("{collectionName}")]
        public async Task Delete(string collectionName)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
            await _client.DeleteDocumentCollectionAsync(collectionUri);
        }
    }
}
