using Azure.Data.Tables;
using MyTrace.Helpers;
using MyTrace.Data.Blog;

namespace MyTrace.Data.Blog
{
    public enum UserType
    {
        Admin,
        Anonymous
    }

    public class BlogPost
    {
        public UserType UserType { get; set; } = UserType.Anonymous;
        public string? UserID { get; set; } // Optional for admin and anonymous
        public Guid PostID { get; set; }
        public int PostVersion { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PostDate { get; set; } = DateTime.Now;
        public DateTime? LastEditDate { get; set; }
        public int ViewCount { get; set; }
        public string Body { get; set; } = string.Empty;

        public BlogPost(Guid id)
        {
            PostID = id;
        }

        public async Task SaveAsync()
        {
            TableClient tableClient = new TableStorageHelper().GetTableClient("blog-posts");
            await tableClient.CreateIfNotExistsAsync();

            string partitionKey = GeneratePartitionKey();

            // Query all entities for the partition key
            //List<BlogPostEntity> blogPostEntities = await tableClient.QueryAsync<BlogPostEntity>(e => e.PartitionKey == partitionKey).ToListAsync();
            List<BlogPostEntity> blogPostEntities = [];
            // Filter in memory to get the latest version of the post
            var latestPost = blogPostEntities.Where(e => e.RowKey.StartsWith(PostID.ToString()))
                                             .OrderByDescending(e => e.ToBlogPost().PostVersion)
                                             .FirstOrDefault();

            if (latestPost != null)
            {
                // If the post exists, increment the PostVersion and update the LastEditDate
                this.PostVersion = latestPost.ToBlogPost().PostVersion + 1;
                this.LastEditDate = DateTime.Now;
            }
            else
            {
                // If the post does not exist, set the initial PostVersion and PostDate
                this.PostVersion = 1;
                this.PostDate = DateTime.Now;
            }

            // Save the new version of the post
            await tableClient.UpsertEntityAsync(new BlogPostEntity(this));
        }

        public async Task LoadAsync()
        {
            TableClient tableClient = new TableStorageHelper().GetTableClient("blog-posts");
            await tableClient.CreateIfNotExistsAsync();

            string partitionKey = GeneratePartitionKey();

            // Query all entities for the partition key
            List<BlogPostEntity> blogPostEntities = await tableClient.QueryAsync<BlogPostEntity>(e => e.PartitionKey == partitionKey)
                                                                     .ToListAsync();

            // Filter in memory to get the latest version of the post
            var latestPost = blogPostEntities.Where(e => e.RowKey.StartsWith(PostID.ToString()))
                                             .OrderByDescending(e => e.ToBlogPost().PostVersion)
                                             .FirstOrDefault();

            if (latestPost != null)
            {
                BlogPost loadedPost = latestPost.ToBlogPost();
                this.PostID = loadedPost.PostID;
                this.PostVersion = loadedPost.PostVersion;
                this.Title = loadedPost.Title;
                this.LastEditDate = loadedPost.LastEditDate;
                this.ViewCount = loadedPost.ViewCount;
                this.Body = loadedPost.Body;
                this.UserType = loadedPost.UserType;
                this.UserID = loadedPost.UserID;
                this.PostDate = loadedPost.PostDate;
            }
        }

        public string GeneratePartitionKey()
        {
            return UserType switch
            {
                UserType.Admin => "Admin",
                UserType.Anonymous => "Anonymous",
                _ => throw new InvalidOperationException("Invalid user type")
            };
        }
    }
}
