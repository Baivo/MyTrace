using Azure;
using Azure.Data.Tables;

namespace MyTrace.Data.Woolworths
{
    public class WooliesStockCodeBarcodeTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public string StockCode { get; set; }
        public string BarCode { get; set; }
        public string Merchant { get; set; }

        public WooliesStockCodeBarcodeTableEntity(WooliesProduct product)
        {
            PartitionKey = product.Product?.Barcode?.ToString() ?? "UnknownBarCode";
            RowKey = product.Product?.Stockcode.ToString() ?? "UnknownStockCode";
            StockCode = product.Product?.Stockcode.ToString() ?? "UnknownStockCode";
            BarCode = product.Product?.Barcode?.ToString() ?? "UnknownBarCode";
            Merchant = "Woolies";
        }
        public WooliesStockCodeBarcodeTableEntity() 
        {
            PartitionKey = "UnknownBarCode";
            RowKey = "UnknownStockCode";
            StockCode = "UnknownStockCode";
            BarCode = "UnknownBarCode";
            Merchant = "Woolies";
        }
    }
}
