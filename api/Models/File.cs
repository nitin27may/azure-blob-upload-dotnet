namespace AzureBlobApi.Models
{
    public class BlobFile
    {
        public string Name { get; set; }

        public DateTimeOffset? CreatedOn { get; set; }

        public Uri Uri { get; set; }
    }
}
