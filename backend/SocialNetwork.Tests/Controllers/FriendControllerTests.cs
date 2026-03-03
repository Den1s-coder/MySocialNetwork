using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities.Users;
using System.Security.Claims;

namespace SocialNetwork.Tests.Controllers;

public class FriendControllerTests
{
    private readonly Mock<IFriendService> _friendServiceMock;
    private readonly Mock<ILogger<FriendController>> _loggerMock;
    private readonly FriendController _friendController;

    public FriendControllerTests()
    {
        _friendServiceMock = new Mock<IFriendService>();
        _loggerMock = new Mock<ILogger<FriendController>>();

        _friendController = new FriendController(_loggerMock.Object, _friendServiceMock.Object);
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

        _friendController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupUserWithEmptyGuid()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, Guid.Empty.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _friendController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupAnonymousUser()
    {
        _friendController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task GetAllFriends_ShouldReturnOk_WithFriendships()
    {
        // Arrange
        var expectedFriendships = new List<Friendship>
        {
            new Friendship { RequesterId = Guid.NewGuid(), AddresseeId = Guid.NewGuid() },
            new Friendship { RequesterId = Guid.NewGuid(), AddresseeId = Guid.NewGuid() }
        };

        _friendServiceMock.Setup(s => s.GetAllFriends(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFriendships);

        // Act
        var result = await _friendController.GetAllFriends();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Friendship>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetAllFriends_ShouldReturnOk_WhenEmpty()
    {
        // Arrange
        _friendServiceMock.Setup(s => s.GetAllFriends(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Friendship>());

        // Act
        var result = await _friendController.GetAllFriends();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Friendship>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetFriendsOfUser_ShouldReturnOk_WithFriends()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var expectedFriendships = new List<Friendship>
        {
            new Friendship { RequesterId = userId, AddresseeId = Guid.NewGuid() }
        };

        _friendServiceMock.Setup(s => s.GetFriendsOfUser(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFriendships);

        // Act
        var result = await _friendController.GetFriendsOfUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Friendship>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetPendingFriendRequests_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var expectedRequests = new List<FriendRequestDto>
        {
            new FriendRequestDto { RequesterId = Guid.NewGuid(), ReceiverId = userId, RequestedAt = DateTime.UtcNow }
        };

        _friendServiceMock.Setup(s => s.GetPendingFriendRequests(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRequests);

        // Act
        var result = await _friendController.GetPendingFriendRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<FriendRequestDto>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetPendingFriendRequests_ShouldReturnUnauthorized_WhenUserIdIsEmpty()
    {
        // Arrange
        SetupUserWithEmptyGuid();

        // Act
        var result = await _friendController.GetPendingFriendRequests();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetMyFriends_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var expectedFriendships = new List<Friendship>
        {
            new Friendship { RequesterId = userId, AddresseeId = Guid.NewGuid() }
        };

        _friendServiceMock.Setup(s => s.GetFriendsOfUser(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFriendships);

        // Act
        var result = await _friendController.GetMyFriends();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Friendship>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetMyFriends_ShouldReturnUnauthorized_WhenUserIdIsEmpty()
    {
        // Arrange
        SetupUserWithEmptyGuid();

        // Act
        var result = await _friendController.GetMyFriends();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task SendFriendRequest_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        SetupUser(userId);

        _friendServiceMock.Setup(s => s.SendFriendRequest(It.IsAny<FriendRequestDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _friendController.SendFriendRequest(addresseeId);

        // Assert
        Assert.IsType<OkResult>(result);
        _friendServiceMock.Verify(s => s.SendFriendRequest(
            It.Is<FriendRequestDto>(dto => dto.RequesterId == userId && dto.ReceiverId == addresseeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendFriendRequest_ShouldReturnUnauthorized_WhenUserIdIsEmpty()
    {
        // Arrange
        SetupUserWithEmptyGuid();

        // Act
        var result = await _friendController.SendFriendRequest(Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _friendServiceMock.Verify(s => s.SendFriendRequest(It.IsAny<FriendRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AcceptFriendRequest_ShouldReturnOk_WhenRequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        _friendServiceMock.Setup(s => s.AcceptFriendRequest(requestId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _friendController.AcceptFriendRequest(requestId);

        // Assert
        Assert.IsType<OkResult>(result);
        _friendServiceMock.Verify(s => s.AcceptFriendRequest(requestId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeclineFriendRequest_ShouldReturnOk_WhenRequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        _friendServiceMock.Setup(s => s.DeclineFriendRequest(requestId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _friendController.DeclineFriendRequest(requestId);

        // Assert
        Assert.IsType<OkResult>(result);
        _friendServiceMock.Verify(s => s.DeclineFriendRequest(requestId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFriend_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var friendId = Guid.NewGuid();
        SetupUser(userId);

        _friendServiceMock.Setup(s => s.RemoveFriend(userId, friendId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _friendController.RemoveFriend(friendId);

        // Assert
        Assert.IsType<OkResult>(result);
        _friendServiceMock.Verify(s => s.RemoveFriend(userId, friendId), Times.Once);
    }

    [Fact]
    public async Task RemoveFriend_ShouldReturnUnauthorized_WhenUserIdIsEmpty()
    {
        // Arrange
        SetupUserWithEmptyGuid();

        // Act
        var result = await _friendController.RemoveFriend(Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _friendServiceMock.Verify(s => s.RemoveFriend(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}