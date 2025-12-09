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

        public FileController(ICloudStorageService storage, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _storage = storage;
            _blobServiceClient = blobServiceClient;
            _containerName = configuration.GetValue<string>("AzureStorage:ContainerName");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName);
            var filename = $"{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            await _storage.UploadFileAsync(stream, filename, file.ContentType);

            var fileUrl = $"{Request.Scheme}://{Request.Host}/api/File/{filename}";
            return Ok(new { filename, fileUrl });
        }

        [HttpGet("{filename}")]
        public async Task<IActionResult> GetFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest();

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var exists = await blobClient.ExistsAsync();
            if (!exists.Value) return NotFound();

            var download = await blobClient.DownloadAsync();
            var contentType = download.Value.ContentType ?? "application/octet-stream";
            var stream = download.Value.Content;

            Response.Headers["Cache-Control"] = "public, max-age=3600";

            return File(stream, contentType);
        }

        [HttpDelete("{filename}")]
        public async Task<IActionResult> DeleteFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest();

            await _storage.DeleteFileAsync(filename);
            return NoContent();
        }
    }
}