# Azure-CosmosDb-Integracao-CSharp
Exemplo de integração com o Azure CosmosDb com SDK para C#

## Azure CosmosDb Emulator

A Microsoft disponibiliza um emulador para o Azure CosmosDb que podemos utilizar sem a necessidade de criar uma conta no Azure.
Para isso podemos fazer download e instalar o Emulador a partir da documentação oficial do Azure CosmosDb.

https://docs.microsoft.com/pt-br/azure/cosmos-db/local-emulator

Após instalar o emulador, acesse o endereço 

https://localhost:8081/_explorer/index.html

Lá estará a URL e a Account Key que vamos precisar para conectar no CosmosDb  

![image](https://user-images.githubusercontent.com/30643035/113885120-928f9b80-978d-11eb-91ed-296f97a2fde0.png)

## Instalando o Pacote NuGet

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.DocumentDB.Core" Version="2.13.1" />
  </ItemGroup>
```

Agora vamos criar algumas chaves no arquivo AppSettings.json, vamos criar as chaves AzureCosmosDb:EndpointUrl e AzureCosmosDb:PrimaryKey

```json
{
  ...
  "AzureCosmosDb": {
    "EndpointUrl": "https://localhost:8081",
    "PrimaryKey": "Your-Account-Key-Azure-CosmoDb-Emulator"
  }
}
```

Vamos fazer a injeção de dependências no ConfigureServices para a interface IDocumentClient obtendo as chaves de configuração e passando para o DocumentClient.

```c#
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

    ...

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        var endpointUrl = Configuration.GetSection("AzureCosmosDb:EndpointUrl").Value;
        var primaryKey = Configuration.GetSection("AzureCosmosDb:PrimaryKey").Value;

        services.AddScoped<IDocumentClient>(f => new DocumentClient(
            new Uri(endpointUrl), 
            primaryKey, 
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
    }
    
    ...
```

E vamos criar nossas controllers para gerenciar DataBases, Containers e Documents

## Criando, Lendo e Excluindo DataBases do AzureCosmosDb

```c#
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

...

  public DataBasesController(IDocumentClient client) : base(client) { }

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
  
...
```

## Criando, Lendo e Excluindo Containers do AzureCosmosDb

```c#
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
```

## Criando, lendo, atualizando e excluindo Documents do AzureComosDb

```c#
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
```
