using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

namespace Core.Services.DbOrders
{
    public class CosmosOrderService : ISqlOrderService
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosOrderService(CosmosClient client, IConfiguration config)
        {
            _client = new CosmosClient(config["CosmosDb:Account"], config["CosmosDb:Key"]);
            
            CreateDatabaseAndContainerAsync(_client, config["CosmosDb:DatabaseName"], config["CosmosDb:ContainerName"]).GetAwaiter().GetResult();

            _container = client.GetContainer(config["CosmosDb:DatabaseName"], config["CosmosDb:ContainerName"]);
        }

        private async Task CreateDatabaseAndContainerAsync(CosmosClient client, string dbName, string containerName)
        {
            var database = await client.CreateDatabaseIfNotExistsAsync(dbName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
        }


        public async Task<DbOrder> AddAsync(DbOrder order)
        {
            await _container.CreateItemAsync(order, new PartitionKey(order.id));
            return order;
        }

        public async Task<DbOrder?> GetOrderAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<DbOrder>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<DbOrder>> GetAllAsync()
        {
            var query = _container.GetItemQueryIterator<DbOrder>(new QueryDefinition("SELECT * FROM c"));
            var results = new List<DbOrder>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }

  

        public async Task<bool> UpdateAsync(DbOrder order)
        {
            await _container.UpsertItemAsync(order, new PartitionKey(order.id));
            return true;
        }

        public async Task<bool> DeleteAsync(DbOrder order)
        {
            try
            {
                await _container.DeleteItemAsync<DbOrder>(order.id, new PartitionKey(order.id));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
