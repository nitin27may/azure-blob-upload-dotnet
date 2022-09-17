using AzureBlobApi.Models;

namespace AzureBlobApi.Services
{
    public interface IBlobService
    {
        Task<string> UploadAsync(Stream fileStream, string containerName, string fileName, string contentType);

        Task<List<BlobFile>> GetFiles(string containerName, int? segmentSize);

        // Uri GetSASToken(string containerName, string fileName);
    }
}
