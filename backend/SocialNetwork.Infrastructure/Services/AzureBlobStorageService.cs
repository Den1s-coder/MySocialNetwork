using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Infrastructure.Services
{
    public class AzureBlobStorageService : ICloudStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly StorageSharedKeyCredential? _sharedKeyCredential;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient, string containerName, string connectionString)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = containerName;

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var accountName = parts.FirstOrDefault(p => p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];
            var accountKey = parts.FirstOrDefault(p => p.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];

            if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountKey))
                _sharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);
            else
                _sharedKeyCredential = null;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);

            return blobClient.Uri.ToString();
        }

        public Task DeleteFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.DeleteIfExistsAsync();
        }

        public string GetFileUrl(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            return blobClient.Uri.ToString();
        }
    }
}
