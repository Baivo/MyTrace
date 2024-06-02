using Azure.Data.Tables;
using Azure;
using Newtonsoft.Json;

namespace MyTrace.Data.Blog
{
    public class BlogPostEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } = default!;

        // Store the BlogPost as a JSON string
        public string BlogPostJson { get; set; }

        public BlogPostEntity() { }

        public BlogPostEntity(BlogPost blogPost)
        {
            PartitionKey = blogPost.GeneratePartitionKey();
            RowKey = $"{blogPost.PostID}-{blogPost.PostVersion}";
            BlogPostJson = JsonConvert.SerializeObject(blogPost);
        }

        public BlogPost ToBlogPost()
        {
            return JsonConvert.DeserializeObject<BlogPost>(BlogPostJson);
        }
    }
}
