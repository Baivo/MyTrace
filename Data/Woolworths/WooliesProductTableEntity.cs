using Azure.Data.Tables;
using Azure;
using Newtonsoft.Json;

namespace MyTrace.Data.Woolworths
{
    public class WooliesProductTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string ProductJson { get; set; }
        public string AdditionalAttributesJson { get; set; }
        public string CountryOfOriginLabelJson { get; set; }
        public string NutritionalInformationJson { get; set; }
        public string PrimaryCategoryJson { get; set; }

        public void SetProduct(WooliesProduct product, DateTime eventTime)
        {
            PartitionKey = product.Product?.Stockcode.ToString() ?? "UnknownStockCode";
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d20");  // Reversed timestamp for RowKey

            ProductJson = JsonConvert.SerializeObject(product.Product);
            AdditionalAttributesJson = JsonConvert.SerializeObject(product.AdditionalAttributes);
            CountryOfOriginLabelJson = JsonConvert.SerializeObject(product.CountryOfOriginLabel);
            NutritionalInformationJson = JsonConvert.SerializeObject(product.NutritionalInformation);
            PrimaryCategoryJson = JsonConvert.SerializeObject(product.PrimaryCategory);
        }

        public WooliesProduct GetProduct()
        {
            var product = new WooliesProduct
            {
                Product = JsonConvert.DeserializeObject<Product>(ProductJson),
                AdditionalAttributes = JsonConvert.DeserializeObject<AdditionalAttributes>(AdditionalAttributesJson),
                CountryOfOriginLabel = JsonConvert.DeserializeObject<CountryOfOriginLabel>(CountryOfOriginLabelJson),
                NutritionalInformation = JsonConvert.DeserializeObject<List<NutritionalInformation>>(NutritionalInformationJson),
                PrimaryCategory = JsonConvert.DeserializeObject<PrimaryCategory>(PrimaryCategoryJson)
            };
            return product;
        }
    }
}
