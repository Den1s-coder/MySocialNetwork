using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Domain.Interfaces;
using Azure.Storage.Blobs;
using SocialNetwork.Domain.Enums;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ICloudStorageService _storage;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Dictionary<ContainerType, string> _containers;
        private readonly string _storageUrl;
        private readonly ILogger<FileController> _logger;

        public FileController(ICloudStorageService storage, BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<FileController> logger)
        {
            _storage = storage;
            _blobServiceClient = blobServiceClient;
            _storageUrl = $"https://{configuration.GetValue<string>("AzureStorage:AccountName")}.blob.core.windows.net";
            _logger = logger;

            var containerConfig = configuration.GetSection("AzureStorage:Containers");
            _containers = new Dictionary<ContainerType, string>
            {
                { ContainerType.Avatars, containerConfig.GetValue<string>("Avatars") },
                { ContainerType.PostPhotos, containerConfig.GetValue<string>("PostPhotos") },
                { ContainerType.CommentPhotos, containerConfig.GetValue<string>("CommentPhotos") },
                { ContainerType.MessagePhotos, containerConfig.GetValue<string>("MessagePhotos") }
            };
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string containerType = "Avatars")
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType?.ToLower()))
                return BadRequest(new { message = "Only image files are allowed (JPEG, PNG, GIF, WebP)." });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File size must not exceed 10MB." });

            try
            {
                if (!Enum.TryParse<ContainerType>(containerType, ignoreCase: true, out var container))
                    return BadRequest(new { message = $"Invalid container type. Allowed: {string.Join(", ", Enum.GetNames(typeof(ContainerType)))}" });

                var actualContainerName = _containers[container];
                var containerClient = _blobServiceClient.GetBlobContainerClient(actualContainerName);

                var extension = Path.GetExtension(file.FileName);
                var filename = $"{Guid.NewGuid()}{extension}";

                using var stream = file.OpenReadStream();
                var fileUrl = await _storage.UploadFileAsync(stream, filename, file.ContentType, container);

                _logger.LogInformation("File uploaded successfully: {FileName} -> {FileUrl}", filename, fileUrl);
                return Ok(new { filename, fileUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error uploading file." });
            }
        }

        [HttpGet("{containerType}/{filename}")]
        public async Task<IActionResult> GetFile(string containerType, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(containerType))
                return BadRequest();

            try
            {
                if (!Enum.TryParse<ContainerType>(containerType, ignoreCase: true, out var container))
                    return BadRequest(new { message = $"Invalid container type. Allowed: {string.Join(", ", Enum.GetNames(typeof(ContainerType)))}" });

                var actualContainerName = _containers[container];
                var containerClient = _blobServiceClient.GetBlobContainerClient(actualContainerName);
                var blobClient = containerClient.GetBlobClient(filename);

                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                    return NotFound();

                var download = await blobClient.DownloadAsync();
                var contentType = download.Value.ContentType ?? "application/octet-stream";

                Response.Headers["Cache-Control"] = "public, max-age=86400";
                return File(download.Value.Content, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {ContainerType}/{FileName}", containerType, filename);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{containerType}/{filename}")]
        public async Task<IActionResult> DeleteFile(string containerType, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(containerType))
                return BadRequest(new { message = "Filename and container type cannot be empty." });

            try
            {
                if (!Enum.TryParse<ContainerType>(containerType, ignoreCase: true, out var container))
                    return BadRequest(new { message = $"Invalid container type. Allowed: {string.Join(", ", Enum.GetNames(typeof(ContainerType)))}" });

                await _storage.DeleteFileAsync(filename, container);
                _logger.LogInformation("File deleted successfully: {ContainerType}/{FileName}", containerType, filename);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {ContainerType}/{FileName}", containerType, filename);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting file." });
            }
        }
    }
}