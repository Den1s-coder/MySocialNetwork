namespace SocialNetwork.Domain.Interfaces
{
    public interface ICloudStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteFileAsync(string fileName);
        string GetFileUrl(string fileName);
    }
}
