using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<UserController>> _loggerMock;
    private readonly UserController _userController;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UserController>>();

        _userController = new UserController(_loggerMock.Object, _userServiceMock.Object);
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

        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupAnonymousUser()
    {
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WithUsers()
    {
        // Arrange
        var expectedUsers = new List<UserDto>
        {
            new UserDto { Id = Guid.NewGuid(), Name = "User1", Email = "user1@test.com" },
            new UserDto { Id = Guid.NewGuid(), Name = "User2", Email = "user2@test.com" }
        };

        _userServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userController.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WhenEmpty()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UserDto>());

        // Act
        var result = await _userController.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto { Id = userId, Name = "TestUser", Email = "test@test.com" };

        _userServiceMock.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userController.GetById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userId, returned.Id);
        Assert.Equal("TestUser", returned.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userServiceMock.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _userController.GetById(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var email = "test@test.com";
        var expectedUser = new UserDto { Id = Guid.NewGuid(), Name = "TestUser", Email = email };

        _userServiceMock.Setup(s => s.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userController.GetByEmail(email);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(email, returned.Email);
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetUserByEmailAsync("none@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _userController.GetByEmail("none@test.com");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByUserName_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var username = "TestUser";
        var expectedUser = new UserDto { Id = Guid.NewGuid(), Name = username, Email = "test@test.com" };

        _userServiceMock.Setup(s => s.GetUserByNameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userController.GetByUserName(username);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(username, returned.Name);
    }

    [Fact]
    public async Task GetByUserName_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetUserByNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _userController.GetByUserName("Unknown");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var expectedUser = new UserDto { Id = userId, Name = "Me", Email = "me@test.com" };

        _userServiceMock.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userController.GetProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userId, returned.Id);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _userController.GetProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _userServiceMock.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _userController.GetProfile();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var updatedDto = new UserDto { Name = "Updated Name", Email = "updated@test.com" };

        _userServiceMock.Setup(s => s.UpdateProfileAsync(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userController.UpdateProfile(updatedDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _userServiceMock.Verify(s => s.UpdateProfileAsync(
            It.Is<UserDto>(dto => dto.Id == userId && dto.Name == "Updated Name"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var changePasswordDto = new ChangePasswordDto("OldPassword123", "NewPassword456");

        _userServiceMock.Setup(s => s.ChangePasswordAsync(userId, changePasswordDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userController.ChangePassword(changePasswordDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _userServiceMock.Verify(s => s.ChangePasswordAsync(userId, changePasswordDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeEmail_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var changeEmailDto = new ChangeEmailDto("newemail@test.com", "Password123");

        _userServiceMock.Setup(s => s.ChangeEmailAsync(userId, changeEmailDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userController.ChangeEmail(changeEmailDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _userServiceMock.Verify(s => s.ChangeEmailAsync(userId, changeEmailDto, It.IsAny<CancellationToken>()), Times.Once);
    }
}