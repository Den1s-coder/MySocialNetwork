using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Tests.Controllers;

public class FileControllerTests
{
    private readonly Mock<ICloudStorageService> _storageMock;
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly Mock<BlobContainerClient> _blobContainerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly FileController _fileController;

    public FileControllerTests()
    {
        _storageMock = new Mock<ICloudStorageService>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobContainerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AzureStorage:ContainerName", "test-container" }
            })
            .Build();

        _blobServiceClientMock.Setup(b => b.GetBlobContainerClient("test-container"))
            .Returns(_blobContainerClientMock.Object);

        _blobContainerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        _fileController = new FileController(
            _storageMock.Object,
            _blobServiceClientMock.Object,
            configuration);

        _fileController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ---------- UploadFile ----------

    [Fact]
    public async Task UploadFile_ShouldReturnOk_WithFilenameAndUrl()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(content);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("photo.png");
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        _storageMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "image/png"))
            .ReturnsAsync("url");

        // Act
        var result = await _fileController.UploadFile(fileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var filenameProp = okResult.Value!.GetType().GetProperty("filename");
        var fileUrlProp = okResult.Value!.GetType().GetProperty("fileUrl");
        Assert.NotNull(filenameProp);
        Assert.NotNull(fileUrlProp);

        var filename = filenameProp!.GetValue(okResult.Value) as string;
        Assert.NotNull(filename);
        Assert.EndsWith(".png", filename);

        _storageMock.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(),
            It.Is<string>(fn => fn.EndsWith(".png")),
            "image/png"), Times.Once);
    }

    [Fact]
    public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsNull()
    {
        // Act
        var result = await _fileController.UploadFile(null!);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequest.Value);
    }

    [Fact]
    public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("empty.txt");

        // Act
        var result = await _fileController.UploadFile(fileMock.Object);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequest.Value);
    }

    // ---------- GetFile ----------

    [Fact]
    public async Task GetFile_ShouldReturnFile_WhenBlobExists()
    {
        // Arrange
        var filename = "test-file.png";
        var fileContent = new byte[] { 10, 20, 30 };
        var contentStream = new MemoryStream(fileContent);

        var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(
            content: contentStream,
            contentType: "image/png");

        var responseMock = new Mock<Response<BlobDownloadInfo>>();
        responseMock.Setup(r => r.Value).Returns(blobDownloadInfo);

        var existsResponseMock = new Mock<Response<bool>>();
        existsResponseMock.Setup(r => r.Value).Returns(true);

        _blobClientMock.Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsResponseMock.Object);

        _blobClientMock.Setup(b => b.DownloadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _fileController.GetFile(filename);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/png", fileResult.ContentType);
    }

    [Fact]
    public async Task GetFile_ShouldReturnNotFound_WhenBlobDoesNotExist()
    {
        // Arrange
        var existsResponseMock = new Mock<Response<bool>>();
        existsResponseMock.Setup(r => r.Value).Returns(false);

        _blobClientMock.Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsResponseMock.Object);

        // Act
        var result = await _fileController.GetFile("nonexistent.png");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetFile_ShouldReturnBadRequest_WhenFilenameIsEmpty()
    {
        // Act
        var result = await _fileController.GetFile("");

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task GetFile_ShouldReturnBadRequest_WhenFilenameIsWhitespace()
    {
        // Act
        var result = await _fileController.GetFile("   ");

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    // ---------- DeleteFile ----------

    [Fact]
    public async Task DeleteFile_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var filename = "file-to-delete.png";

        _storageMock.Setup(s => s.DeleteFileAsync(filename))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _fileController.DeleteFile(filename);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _storageMock.Verify(s => s.DeleteFileAsync(filename), Times.Once);
    }

    [Fact]
    public async Task DeleteFile_ShouldReturnBadRequest_WhenFilenameIsEmpty()
    {
        // Act
        var result = await _fileController.DeleteFile("");

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _storageMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFile_ShouldReturnBadRequest_WhenFilenameIsWhitespace()
    {
        // Act
        var result = await _fileController.DeleteFile("   ");

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _storageMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }
}