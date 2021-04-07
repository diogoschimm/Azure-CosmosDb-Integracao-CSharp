using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppAzureCosmosDbSample.Models;

namespace WebAppAzureCosmosDbSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : BaseApi
    {
        private readonly string DatabaseName = "DbFornecedores";
        private readonly string CollectionName = "Suppliers";
        private readonly Uri CollectionUri;

        public DocumentsController(IDocumentClient client) : base(client)
        {
            CollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
        }

        [HttpGet]
        public async Task<IEnumerable<dynamic>> Get()
        {
            var documents = await _client.ReadDocumentFeedAsync(CollectionUri);

            return documents.ToList().Select(c => new { c.id, c.supplierId, c.name, c.documentNumber, c.documentType });
        }

        [HttpGet("sup/{supplierId}")]
        public Supplier GetSupplierId(string supplierId)
        {
            var query = new SqlQuerySpec(
                $"SELECT * FROM Supplier s where s.supplierId = @SupplierId",
                new SqlParameterCollection(new SqlParameter[] {
                    new SqlParameter
                    {
                        Name = "@SupplierId",
                        Value = supplierId
                    }
                }));

            var suppliers = _client.CreateDocumentQuery<Supplier>(CollectionUri, query);
            return suppliers.ToList().FirstOrDefault();
        }

        [HttpGet("query")]
        public IEnumerable<Supplier> GetAllQuery()
        {
            var suppliers = _client.CreateDocumentQuery<Supplier>(CollectionUri, "SELECT * FROM Supplier");
            return suppliers;
        }

        [HttpGet("query/{name}")]
        public IEnumerable<Supplier> GetByName(string name)
        {
            var query = new SqlQuerySpec(
                $"SELECT * FROM Supplier s where s.name = @name",
                new SqlParameterCollection(new SqlParameter[] {
                    new SqlParameter
                    {
                        Name = "@name",
                        Value = name
                    }
                }));

            var suppliers = _client.CreateDocumentQuery<Supplier>(CollectionUri, query);
            return suppliers;
        }

        [HttpGet("{documentId}")]
        public async Task<Supplier> Get(string documentId)
        {
            var uriDocument = UriFactory.CreateDocumentUri(DatabaseName, CollectionName, documentId);

            var document = await _client.ReadDocumentAsync(uriDocument);
            return JsonConvert.DeserializeObject<Supplier>(document.Resource.ToString());
        }

        [HttpPost]
        public async Task<dynamic> Post([FromBody] Supplier supplier)
        {
            var document = await _client.CreateDocumentAsync(CollectionUri, supplier);
            return new { id = document.Resource.Id, supplier };
        }

        [HttpPut("{documentId}")]
        public async Task<dynamic> Put(string documentId, [FromBody] Supplier supplier)
        {
            supplier.Id = documentId;
            var uriDocument = UriFactory.CreateDocumentUri(DatabaseName, CollectionName, documentId);

            var document = await _client.ReplaceDocumentAsync(uriDocument, supplier);
            return new { id = document.Resource.Id, supplier };
        }

        [HttpDelete("{documentId}")]
        public async Task Delete(string documentId)
        {
            var uriDocument = UriFactory.CreateDocumentUri(DatabaseName, CollectionName, documentId);
            await _client.DeleteDocumentAsync(uriDocument);
        }
    }
}
