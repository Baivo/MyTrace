using MyTraceLib.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTrace.Services
{
    public static class FunctionsService
    {
        private static readonly string FunctionsCode = "jX7ieFKp6eNhVUHzQEUqq4rt7XRczZ-J3etU7jmbEGHVAzFuHhhsYQ==";
        private static readonly string BaseUrl = "https://mytracefunctions.azurewebsites.net/api/";

        public static async Task<WoolworthsProduct?> RequestProductByBarcodeAsync(string barcode)
        {
            using (HttpClient client = new HttpClient())
            {
                string functionUrl = $"{BaseUrl}RequestProductByBarcode?code={FunctionsCode}&barcode={barcode}";
                HttpResponseMessage response = await client.GetAsync(functionUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    // Check if the response content indicates that the product was not found
                    if (content.Contains("Product not found"))
                    {
                        return null;
                    }

                    WoolworthsProduct? product = JsonConvert.DeserializeObject<WoolworthsProduct>(content);
                    return product;
                }
                else
                {
                    // Handle the error as needed
                    return null;
                }
            }
        }
        public static async Task<List<WoolworthsProduct>> GetSimilarProductsAsync(string barcode)
        {
            using (HttpClient client = new HttpClient())
            {
                string functionUrl = $"{BaseUrl}GetSimilarProducts?code={FunctionsCode}&barcode={barcode}";
                HttpResponseMessage response = await client.GetAsync(functionUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    // Check if the response content indicates that no matching products were found
                    if (content.Contains("Product not found"))
                    {
                        return new List<WoolworthsProduct>();
                    }
                    if (content.Contains("No matching products found"))
                    {
                        return new List<WoolworthsProduct>();
                    }

                    List<WoolworthsProduct> matchingProducts = JsonConvert.DeserializeObject<List<WoolworthsProduct>>(content);
                    return matchingProducts;
                }
                else
                {
                    // Handle the error as needed
                    return new List<WoolworthsProduct>();
                }
            }
        }
    }
}
