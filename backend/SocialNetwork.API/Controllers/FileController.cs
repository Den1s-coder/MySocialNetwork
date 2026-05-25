using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Domain.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ICloudStorageService _storage;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly ILogger<FileController> _logger;

        public FileController(ICloudStorageService storage, BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<FileController> logger)
        {
            _storage = storage;
            _blobServiceClient = blobServiceClient;
            _containerName = configuration.GetValue<string>("AzureStorage:ContainerName");
            _logger = logger;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            // Валідація типу файлу
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType?.ToLower()))
                return BadRequest(new { message = "Only image files are allowed (JPEG, PNG, GIF, WebP)." });

            // Валідація розміру (максимум 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File size must not exceed 10MB." });

            try
            {
                var extension = Path.GetExtension(file.FileName);
                var filename = $"{Guid.NewGuid()}{extension}";

                using var stream = file.OpenReadStream();
                var fileUrl = await _storage.UploadFileAsync(stream, filename, file.ContentType);

                _logger.LogInformation("File uploaded successfully: {FileName}", filename);
                return Ok(new { filename, fileUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error uploading file." });
            }
        }

        [HttpGet("{filename}")]
        public async Task<IActionResult> GetFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest();

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(filename);

                var exists = await blobClient.ExistsAsync();
                if (!exists.Value) 
                    return NotFound();

                var download = await blobClient.DownloadAsync();
                var contentType = download.Value.ContentType ?? "application/octet-stream";
                var stream = download.Value.Content;

                Response.Headers["Cache-Control"] = "public, max-age=86400";

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {FileName}", filename);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{filename}")]
        public async Task<IActionResult> DeleteFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest(new { message = "Filename cannot be empty." });

            try
            {
                await _storage.DeleteFileAsync(filename);
                _logger.LogInformation("File deleted successfully: {FileName}", filename);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", filename);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting file." });
            }
        }
    }
}