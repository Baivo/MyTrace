using Azure.Data.Tables;
using Azure;

namespace MyTrace.Data.Woolworths
{
    public class WooliesProductValidTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public bool IsValid { get; set; }
    }
}
