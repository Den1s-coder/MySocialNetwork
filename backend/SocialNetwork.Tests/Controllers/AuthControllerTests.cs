using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO.Auth;
using SocialNetwork.Application.Interfaces;

namespace SocialNetwork.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _authController = new AuthController(_loggerMock.Object, _authServiceMock.Object);
    }

    // ---------- Register ----------

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "TestUser",
            Email = "test@test.com",
            Password = "Password123"
        };

        _authServiceMock.Setup(s => s.RegisterAsync(registerDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authController.Register(registerDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _authServiceMock.Verify(s => s.RegisterAsync(registerDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenServiceThrows()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "TestUser",
            Email = "test@test.com",
            Password = "Password123"
        };

        _authServiceMock.Setup(s => s.RegisterAsync(registerDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _authController.Register(registerDto));
    }

    // ---------- Login ----------

    [Fact]
    public async Task Login_ShouldReturnOk_WithTokens()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "TestUser", Password = "Password123" };
        var expectedResponse = new LoginResponce("access-token-123", "refresh-token-456");

        _authServiceMock.Setup(s => s.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<LoginResponce>(okResult.Value);
        Assert.Equal("access-token-123", returned.AccessToken);
        Assert.Equal("refresh-token-456", returned.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "Unknown", Password = "Password123" };

        _authServiceMock.Setup(s => s.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException("User not found"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _authController.Login(loginDto));
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordIsInvalid()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "TestUser", Password = "WrongPassword" };

        _authServiceMock.Setup(s => s.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid password"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authController.Login(loginDto));
    }

    // ---------- RefreshToken ----------

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WithNewTokens()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var expectedResponse = new LoginResponce("new-access-token", "new-refresh-token");

        _authServiceMock.Setup(s => s.LoginWithRefreshTokenAsync(refreshToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _authController.RefreshToken(refreshToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<LoginResponce>(okResult.Value);
        Assert.Equal("new-access-token", returned.AccessToken);
        Assert.Equal("new-refresh-token", returned.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldThrow_WhenTokenIsInvalidOrExpired()
    {
        // Arrange
        var refreshToken = "expired-token";

        _authServiceMock.Setup(s => s.LoginWithRefreshTokenAsync(refreshToken))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired refresh token"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authController.RefreshToken(refreshToken));
    }

    // ---------- Google Login ----------

    [Fact]
    public async Task LoginWithGoogle_ShouldReturnOk_WithTokens()
    {
        // Arrange
        var idToken = "valid-google-id-token";
        var expectedResponse = new LoginResponce("google-access-token", "google-refresh-token");

        _authServiceMock.Setup(s => s.LoginWithGoogleAsync(idToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _authController.LoginWithLogin(idToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<LoginResponce>(okResult.Value);
        Assert.Equal("google-access-token", returned.AccessToken);
        Assert.Equal("google-refresh-token", returned.RefreshToken);
    }

    [Fact]
    public async Task LoginWithGoogle_ShouldThrow_WhenTokenIsInvalid()
    {
        // Arrange
        var idToken = "invalid-token";

        _authServiceMock.Setup(s => s.LoginWithGoogleAsync(idToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid Google ID token"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authController.LoginWithLogin(idToken));
    }

    [Fact]
    public async Task LoginWithGoogle_ShouldThrow_WhenIdTokenIsEmpty()
    {
        // Arrange
        var idToken = "";

        _authServiceMock.Setup(s => s.LoginWithGoogleAsync(idToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("ID token is null or empty"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _authController.LoginWithLogin(idToken));
    }
}