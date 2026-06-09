using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Domain.Enums;

namespace SocialNetwork.Infrastructure.Services
{
    public class AzureBlobStorageService : ICloudStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Dictionary<ContainerType, string> _containers;
        private readonly StorageSharedKeyCredential? _sharedKeyCredential;
        private readonly string _apiBaseUrl;
        private readonly string _accountName;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient, Dictionary<ContainerType, string> containers, string connectionString, string apiBaseUrl = null)
        {
            _blobServiceClient = blobServiceClient;
            _containers = containers ?? throw new ArgumentNullException(nameof(containers));
            _apiBaseUrl = apiBaseUrl ?? "https://localhost:7142"; 

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _accountName = parts.FirstOrDefault(p => p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];
            var accountKey = parts.FirstOrDefault(p => p.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];

            if (!string.IsNullOrEmpty(_accountName) && !string.IsNullOrEmpty(accountKey))
                _sharedKeyCredential = new StorageSharedKeyCredential(_accountName, accountKey);
            else
                _sharedKeyCredential = null;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(fileStream, fileName, contentType, ContainerType.Avatars);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, ContainerType containerType)
        {
            if (!_containers.TryGetValue(containerType, out var containerName))
                throw new InvalidOperationException($"Container type {containerType} is not configured.");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);

            return GetFileUrl(fileName, containerType);
        }

        public Task DeleteFileAsync(string fileName)
        {
            return DeleteFileAsync(fileName, ContainerType.Avatars);
        }

        public Task DeleteFileAsync(string fileName, ContainerType containerType)
        {
            if (!_containers.TryGetValue(containerType, out var containerName))
                throw new InvalidOperationException($"Container type {containerType} is not configured.");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.DeleteIfExistsAsync();
        }

        public string GetFileUrl(string fileName)
        {
            return GetFileUrl(fileName, ContainerType.Avatars);
        }

        public string GetFileUrl(string fileName, ContainerType containerType)
        {
            if (!_containers.TryGetValue(containerType, out _))
                throw new InvalidOperationException($"Container type {containerType} is not configured.");

            return $"{_apiBaseUrl.TrimEnd('/')}/api/File/{containerType}/{fileName}";
        }

        public async Task<bool> FileExistsAsync(string fileName, ContainerType containerType)
        {
            if (!_containers.TryGetValue(containerType, out var containerName))
                throw new InvalidOperationException($"Container type {containerType} is not configured.");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var exists = await blobClient.ExistsAsync();
            return exists.Value;
        }
    }
}
