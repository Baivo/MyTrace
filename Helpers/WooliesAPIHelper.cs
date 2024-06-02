using Azure.Data.Tables;
using MyTrace.Data.Woolworths;
using Newtonsoft.Json;
using System.Net;

namespace MyTrace.Helpers
{
    internal class WooliesAPIHelper
    {
        private readonly HttpClient client;
        private bool DebugMode { get; set; } = false;
        public WooliesAPIHelper(bool printMode = false)
        {
            DebugMode = printMode;
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0");
        }

        public async Task InitializeSession()
        {
            var response = await client.GetAsync("https://www.woolworths.com.au");
            response.EnsureSuccessStatusCode();
        }
        public async Task<WooliesProduct?> RequestProductFromStockCodeAsync(int stockCode)
        {
            TableStorageHelper tableStorageService = new();
            bool isSaved = await ProductSavedAsync(stockCode);
            if (isSaved)
            {
                bool isValid = await ProductValidAsync(stockCode);
                if (!isValid)
                {
                    return null;
                }
                else
                {
                    var existingProductEntity = await tableStorageService.GetMostRecentEntityAsync<WooliesProductTableEntity>("woolies-products", stockCode.ToString());
                    WooliesProduct existingProduct = existingProductEntity.GetProduct();
                    return existingProduct;
                }
            }
            else
            {
                return null;
            }
        }
        public async Task<WooliesProduct?> RequestProductFromBarcodeAsync(string barcode)
        {
            WooliesStockCodeBarcodeTableEntity? entity = null;
            TableStorageHelper tableStorageService = new();
            TableClient tableClient = tableStorageService.GetTableClient("stockcode-barcode-index");
            await foreach (var e in tableClient.QueryAsync<WooliesStockCodeBarcodeTableEntity>(e => e.PartitionKey == barcode))
            {
                entity = e;
                break;
            }
            if (entity == null)
                return null;
            else
                return await RequestProductFromStockCodeAsync(int.Parse(entity.RowKey));
        }
        public async Task<WooliesProduct?> RequestProductAsync(int productId)
        {
            try
            {
                var apiUrl = $"https://www.woolworths.com.au/apis/ui/product/detail/{productId}?isMobile=false&useVariant=true";
                var apiResponse = await client.GetAsync(apiUrl);
                if (!apiResponse.IsSuccessStatusCode)
                    return null;

                string responseBody = await apiResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WooliesProduct>(responseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product {productId}: {ex.Message}");
                return null;
            }
        }
        public async Task ProcessProduct(int productId)
        {
            TableStorageHelper tableStorageService = new();
            bool isSaved = await ProductSavedAsync(productId);
            if (isSaved)
            {
                bool isValid = await ProductValidAsync(productId);
                if (!isValid)
                {
                    return;
                }
                else
                {
                    var existingProductEntity = await tableStorageService.GetMostRecentEntityAsync<WooliesProductTableEntity>("woolies-products", productId.ToString());
                    WooliesProduct existingProduct = existingProductEntity.GetProduct();
                    IndexProduct(existingProduct);
                    string barcode = existingProduct.Product?.Barcode ?? string.Empty;
                }

                return;
            }

            WooliesProduct? product = await RequestProductAsync(productId);

            if (product?.Product != null)
            {
                IndexProduct(product);
                var productEntity = new WooliesProductTableEntity();
                productEntity.SetProduct(product, DateTime.UtcNow);
                await tableStorageService.AddEntityAsync("woolies-products", productEntity);

                WooliesProductValidTableEntity productExistsTableEntity = new WooliesProductValidTableEntity
                {
                    PartitionKey = "product-valid",
                    RowKey = productId.ToString(),
                    IsValid = true
                };
                await tableStorageService.AddEntityAsync("woolies-product-check", productExistsTableEntity);
            }
            else
            {
                WooliesProductValidTableEntity productNotExistsTableEntity = new WooliesProductValidTableEntity
                {
                    PartitionKey = "product-valid",
                    RowKey = productId.ToString(),
                    IsValid = false
                };
                await tableStorageService.AddEntityAsync("woolies-product-check", productNotExistsTableEntity);
            }
        }
        private async Task<bool> ProductSavedAsync(int productId)
        {
            TableStorageHelper tableStorageService = new();
            return await tableStorageService.EntityExistsAsync<WooliesProductTableEntity>("woolies-product-check", "product-valid", productId.ToString());
        }
        private static async Task<bool> ProductValidAsync(int productId)
        {
            TableStorageHelper tableStorageService = new();
            var entity = await tableStorageService.GetEntityAsync<WooliesProductValidTableEntity>("woolies-product-check", "product-valid", productId.ToString());
            return entity.IsValid;
        }
        private async void IndexProduct(WooliesProduct product)
        {
            try
            {
                TableStorageHelper tableStorageService = new TableStorageHelper();
                WooliesStockCodeBarcodeTableEntity tableEntity = new WooliesStockCodeBarcodeTableEntity(product);

                TableClient tableClient = tableStorageService.GetTableClient("stockcode-barcode-index");
                await tableClient.CreateIfNotExistsAsync();
                await tableClient.UpsertEntityAsync(tableEntity);
            }
            catch
            {
                return;
            }
        }
        public async Task IndexExistingProductFromIDAsync(int productId)
        {
            TableStorageHelper tableStorageService = new TableStorageHelper();
            bool isSaved = await ProductSavedAsync(productId);
            if (isSaved)
            {
                bool isValid = await ProductValidAsync(productId);
                if (isValid)
                {
                    WooliesProductTableEntity productEntity = await tableStorageService.GetMostRecentEntityAsync<WooliesProductTableEntity>("woolies-products", productId.ToString());
                    WooliesProduct product = productEntity.GetProduct();
                    if (product != null && product.Product != null && product.Product.Barcode != null)
                    {
                        IndexProduct(product);
                    }
                }
            }
        }
    }
}
