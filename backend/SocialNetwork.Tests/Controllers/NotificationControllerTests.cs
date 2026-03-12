using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using System.Security.Claims;

namespace SocialNetwork.Tests.Controllers;

public class NotificationControllerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<NotificationController>> _loggerMock;
    private readonly NotificationController _notificationController;

    public NotificationControllerTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<NotificationController>>();

        _notificationController = new NotificationController(
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    private void SetupUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, userId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _notificationController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupAnonymousUser()
    {
        _notificationController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnOk_WithNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var expectedNotifications = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "New post", IsRead = false },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "New comment", IsRead = true }
        };

        _notificationServiceMock.Setup(s => s.GetUserNotificationsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotifications);

        // Act
        var result = await _notificationController.GetUserNotifications(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Notification>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnOk_WhenEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.GetUserNotificationsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Notification>());

        // Act
        var result = await _notificationController.GetUserNotifications(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Notification>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _notificationController.GetUserNotifications(CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.GetUserNotificationsAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationController.GetUserNotifications(CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetUnreadNotifications_ShouldReturnOk_WithUnreadNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var expectedNotifications = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Unread 1", IsRead = false }
        };

        _notificationServiceMock.Setup(s => s.GetUnreadNotificationsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotifications);

        // Act
        var result = await _notificationController.GetUnreadNotifications(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Notification>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetUnreadNotifications_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _notificationController.GetUnreadNotifications(CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetUnreadNotifications_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.GetUnreadNotificationsAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationController.GetUnreadNotifications(CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.MarkAsReadAsync(userId, notificationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _notificationController.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
        _notificationServiceMock.Verify(s => s.MarkAsReadAsync(userId, notificationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _notificationController.MarkAsRead(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnForbid_WhenUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.MarkAsReadAsync(userId, notificationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _notificationController.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenNotificationNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.MarkAsReadAsync(userId, notificationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Notification not found"));

        // Act
        var result = await _notificationController.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Notification not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturn500_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.MarkAsReadAsync(userId, notificationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _notificationController.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _notificationController.MarkAllAsRead(CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
        _notificationServiceMock.Verify(s => s.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _notificationController.MarkAllAsRead(CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _notificationServiceMock.Setup(s => s.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationController.MarkAllAsRead(CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
}