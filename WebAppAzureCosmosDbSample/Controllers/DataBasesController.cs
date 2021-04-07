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
    public class DataBasesController : BaseApi
    {
        public DataBasesController(IDocumentClient client)
            : base(client) { }

        [HttpGet]
        public async Task<IEnumerable<dynamic>> Get()
        {
            IEnumerable<Database> databases = await _client.ReadDatabaseFeedAsync();
            return databases.ToList().Select(d => new { id = d.Id });
        }

        [HttpGet("{databaseName}")]
        public async Task<string> Get(string databaseName)
        {
            Database database = await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            return database.Id;
        }

        [HttpPost]
        public async Task Post([FromBody] string databaseName)
        {
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });
        }

        [HttpDelete("{databaseName}")]
        public async Task Delete(string databaseName)
        {
            await _client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
        }
    }
}
