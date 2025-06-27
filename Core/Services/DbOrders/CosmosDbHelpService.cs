using Microsoft.Azure.Cosmos;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace Core.Services.DbOrders
{
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(string account, string key, string databaseName, string containerName)
        {
            var client = new CosmosClient(account, key);
            CreateDatabaseAndContainerAsync(client, databaseName, containerName).Wait();
            _container = client.GetContainer(databaseName, containerName);
        }

        private async Task CreateDatabaseAndContainerAsync(CosmosClient client, string dbName, string containerName)
        {
            var database = await client.CreateDatabaseIfNotExistsAsync(dbName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
        }

        public async Task AddItemAsync<T>(T item) where T : class
        {  
            var idProp = item.GetType().GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            if (idProp == null)
                throw new InvalidOperationException("The item does not contain a property named 'id'.");

            var idValue = idProp.GetValue(item)?.ToString();

            try
            {
                await _container.CreateItemAsync(item, new PartitionKey(idValue));
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.StatusCode} - {ex.Message}");
                throw;
            }


        }
    }
}
