using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace WebAppAzureCosmosDbSample.Controllers
{
    public abstract  class BaseApi : ControllerBase
    { 
        protected readonly IDocumentClient _client;

        protected BaseApi(IDocumentClient client)
        {
            this._client = client;
        }
    }
}
