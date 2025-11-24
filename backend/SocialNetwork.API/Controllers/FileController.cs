using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ICloudStorageService _storage;

        public FileController(ICloudStorageService storage)
        {
            _storage = storage;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var extension = Path.GetExtension(file.FileName);
            var filename = $"{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            var fileUrl = await _storage.UploadFileAsync(stream, file.FileName, file.ContentType);
            return Ok(new { filename, fileUrl });
        }
    }
}
