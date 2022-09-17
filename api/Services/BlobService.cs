using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using AzureBlobApi.Controllers;
using AzureBlobApi.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace AzureBlobApi.Services
{
    public class BlobService : IBlobService
    {
        private readonly string _storageConnectionString;
        private readonly ILogger _logger;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _storageConnectionString = configuration.GetConnectionString("AzureStorage");
            _logger = logger;
        }

        public async Task<List<BlobFile>> GetFiles(string containerName, int? segmentSize)
        {
            if (!IsContainerNameValid(containerName))
            {
                _logger.LogError($"Invalid Container Name: {containerName}.");

                throw new HttpStatusException(HttpStatusCode.BadRequest, $"Invalid Container Name: {containerName}");
            }

            List<BlobFile> files = new List<BlobFile>();
            BlobContainerClient blobContainerClient = new BlobContainerClient(_storageConnectionString, containerName);

            bool isExists = blobContainerClient.Exists();

            if (!isExists)
            {
                _logger.LogError($"Container {containerName} doest not exist.");

                throw new HttpStatusException(HttpStatusCode.NotFound, $"Container {containerName} doest not exist.");
            }

            var resultSegment = blobContainerClient.GetBlobsAsync().AsPages(default, segmentSize);

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {

                    BlobClient blob = blobContainerClient.GetBlobClient(blobItem.Name);
                    files.Add( new BlobFile{Name =  blobItem.Name, Uri = GetSASToken(blob), CreatedOn = blobItem.Properties.CreatedOn });
                }
            }

           return files;
        }

        public Uri GetSASToken(BlobClient blobClient)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
            sasBuilder.SetPermissions(BlobSasPermissions.Read |
                BlobSasPermissions.Write);

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri;
        }

        public async Task<string> UploadAsync(Stream fileStream, string containerName, string fileName, string contentType)
        {
            if (!IsContainerNameValid(containerName))
            {
                _logger.LogError($"Invalid Container Name: {containerName}.");

                throw new HttpStatusException(HttpStatusCode.BadRequest, $"Invalid Container Name: {containerName}");
            }

            // Get a credential and create a client object for the blob container.
            BlobContainerClient containerClient = new BlobContainerClient(_storageConnectionString, containerName);

            try
            {
                // Create the container if it does not exist.
                await containerClient.CreateIfNotExistsAsync();

                await containerClient.SetAccessPolicyAsync(PublicAccessType.None);
                var blob = containerClient.GetBlobClient(fileName);
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
                var url = GetSASToken(blob);
                return url.ToString();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error While uploading file: {fileName} in container: {containerName}", exception.Message);

                throw new HttpStatusException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        public bool IsContainerNameValid(string name)
        {
            return Regex.IsMatch(name, "^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        //public bool ContainerExists(string name)
        //{
        //    return (IsContainerNameValid(name) ? _blobServiceClient.GetBlobContainerClient(name).Exists() : false);
        //}
    }
}