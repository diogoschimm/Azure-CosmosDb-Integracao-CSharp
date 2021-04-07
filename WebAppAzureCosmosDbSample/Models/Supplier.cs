using Newtonsoft.Json;
using System;

namespace WebAppAzureCosmosDbSample.Models
{
    public class Supplier
    {
        public Supplier()
        {
            SupplierId = Guid.NewGuid().ToString();
        }
         
        public string Id { get; set; } 
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
    }
}
