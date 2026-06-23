using SocialNetwork.Domain.Enums;

namespace SocialNetwork.Domain.Interfaces
{
    public interface ICloudStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, ContainerType containerType);
        Task DeleteFileAsync(string fileName);
        Task DeleteFileAsync(string fileName, ContainerType containerType);
        string GetFileUrl(string fileName);
        string GetFileUrl(string fileName, ContainerType containerType);
        Task<bool> FileExistsAsync(string fileName, ContainerType containerType);
    }
}
