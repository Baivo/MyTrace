using Azure.Data.Tables;
using Azure;

namespace MyTrace.Helpers
{
    public interface ITableStorageService
    {
        Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
        Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new();
        Task<List<T>> QueryEntitiesAsync<T>(string tableName, string filter) where T : class, ITableEntity, new();
        Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
        Task DeleteEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
    }
    public class TableStorageHelper : ITableStorageService
    {
        private string ConnectionString = @"DefaultEndpointsProtocol=https;AccountName=runedb;AccountKey=qSQWOOCIkHta50YFE06FAFElbJr6EmgDmLQQe9t7ofMrJf9hROIkZRH3Jy05a9isw5QZLsWmQButACDbg5oLQQ==;TableEndpoint=https://runedb.table.cosmos.azure.com:443/;";

        private readonly TableServiceClient _tableServiceClient;

        public TableStorageHelper()
        {
            _tableServiceClient = new TableServiceClient(ConnectionString);
        }
        public TableStorageHelper(string connectionString)
        {
            ConnectionString = connectionString;
            _tableServiceClient = new TableServiceClient(ConnectionString);
        }
        public TableClient GetTableClient(string tableName)
        {
            return new TableClient(ConnectionString, tableName);
        }

        public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();
                await tableClient.UpsertEntityAsync(entity);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var entities = new List<T>();

                await foreach (var entity in tableClient.QueryAsync<T>())
                {
                    entities.Add(entity);
                }

                return entities;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<List<T>> QueryEntitiesAsync<T>(string tableName, string filter) where T : class, ITableEntity, new()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var entities = new List<T>();
                await foreach (var entity in tableClient.QueryAsync<T>(filter))
                {
                    entities.Add(entity);
                }
                return entities;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404)
                {
                    return null;
                }
                Console.WriteLine($"Error retrieving entity: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EntityExistsAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var entity = await GetEntityAsync<T>(tableName, partitionKey, rowKey);
            return entity != null;
        }

        public async Task<T?> GetMostRecentEntityAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            string filter = $"PartitionKey eq '{partitionKey}'";
            // Query the table and order results by RowKey in descending order to get the most recent entity first.
            var queryResults = tableClient.QueryAsync<T>(filter).AsPages(default, 1); // Requesting only one page with one item

            await foreach (var page in queryResults)
            {
                // Return the first item which is the most recent one due to our RowKey design
                return page.Values.FirstOrDefault();
            }
            return null;
        }

        public async Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.UpsertEntityAsync(entity);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task DeleteEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, entity.ETag);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public async Task BulkUploadToTableAsync<T>(List<T> entities, string tableName) where T : ITableEntity
        {
            TableStorageHelper tableStorageService = new TableStorageHelper();
            TableClient tableClient = tableStorageService.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();

            string partitionKey = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            const int batchSize = 100;
            int maxRetries = 5;
            double backoffFactor = 2.0;
            int totalEntities = entities.Count;
            int entitiesProcessed = 0;

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Console.WriteLine($"Starting bulk upload of {totalEntities} entities to {tableName}.");

            for (int i = 0; i < totalEntities; i += batchSize)
            {
                List<TableTransactionAction> batch = new List<TableTransactionAction>();
                for (int j = 0; j < batchSize && i + j < totalEntities; j++)
                {
                    var entity = entities[i + j];
                    entity.PartitionKey = partitionKey;
                    entity.RowKey = entity.RowKey ?? Guid.NewGuid().ToString();

                    batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertMerge, entity));
                }

                int retryCount = 0;
                while (true)
                {
                    try
                    {
                        await tableClient.SubmitTransactionAsync(batch);
                        entitiesProcessed += batch.Count;
                        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        double averageTimePerEntity = elapsedSeconds / entitiesProcessed;
                        double estimatedTotalTime = averageTimePerEntity * totalEntities;
                        double estimatedTimeRemaining = estimatedTotalTime - elapsedSeconds;

                        Console.Write($"\rSuccessfully uploaded {entitiesProcessed}/{totalEntities} entities. " +
                                      $"Elapsed Time: {elapsedSeconds:F2} seconds. " +
                                      $"Estimated Time Remaining: {estimatedTimeRemaining:F2} seconds.");

                        break;
                    }
                    catch (RequestFailedException ex) when (ex.Status == 429 && retryCount < maxRetries)
                    {
                        Console.Write($"\rRate limit exceeded, retrying... Attempt {retryCount + 1}. ");
                        await Task.Delay((int)(Math.Pow(backoffFactor, retryCount) * 1000));
                        retryCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.Write($"\rAn error occurred: {ex.Message}. ");
                        break;
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"\nBulk upload to {tableName} complete. Total time: {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }
    }
}
