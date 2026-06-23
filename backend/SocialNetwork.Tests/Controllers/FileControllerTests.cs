using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private readonly Mock<ILogger<FileController>> _logger;

    public FileControllerTests()
    {
        _storageMock = new Mock<ICloudStorageService>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobContainerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();
        _logger = new Mock<ILogger<FileController>>();

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
            configuration,
            _logger.Object);

        _fileController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    //TODO
}