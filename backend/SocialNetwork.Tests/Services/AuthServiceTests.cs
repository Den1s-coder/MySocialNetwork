using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using SocialNetwork.Application.DTO.Auth;
using SocialNetwork.Application.Mappings;
using SocialNetwork.Application.Service;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace SocialNetwork.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly IMapper _mapper;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        _mapper = config.CreateMapper();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:GoogleClientId", "test-google-client-id" }
            })
            .Build();

        _authService = new AuthService(
            _userRepoMock.Object,
            _mapper,
            _jwtProviderMock.Object,
            _refreshTokenRepoMock.Object,
            configuration);
    }

    private User CreateUserWithHashedPassword(string name, string email, string password)
    {
        var user = new User(name, email, "placeholder");
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        return user;
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenDtoIsValid()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "NewUser",
            Email = "new@test.com",
            Password = "Password123"
        };

        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        _userRepoMock.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Name == "NewUser" && u.Email == "new@test.com" && u.PasswordHash != "Password123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "User",
            Email = "user@test.com",
            Password = "PlainPassword"
        };

        User? capturedUser = null;
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotEqual("PlainPassword", capturedUser!.PasswordHash);

        var verifyResult = new PasswordHasher<User>()
            .VerifyHashedPassword(capturedUser, capturedUser.PasswordHash, "PlainPassword");
        Assert.Equal(PasswordVerificationResult.Success, verifyResult);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenDtoIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _authService.RegisterAsync(null!));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateUserWithHashedPassword("TestUser", "test@test.com", "CorrectPassword");

        _userRepoMock.Setup(r => r.GetByUserNameAsync("TestUser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtProviderMock.Setup(j => j.GenerateAccessToken(user))
            .Returns("access-token");
        _jwtProviderMock.Setup(j => j.GenerateRefreshToken())
            .Returns("refresh-token");

        _refreshTokenRepoMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        var loginDto = new LoginDto { Username = "TestUser", Password = "CorrectPassword" };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        _refreshTokenRepoMock.Verify(r => r.CreateAsync(
            It.Is<RefreshToken>(rt => rt.Token == "refresh-token" && rt.UserId == user.Id)),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByUserNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var loginDto = new LoginDto { Username = "Unknown", Password = "Password" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateUserWithHashedPassword("TestUser", "test@test.com", "CorrectPassword");

        _userRepoMock.Setup(r => r.GetByUserNameAsync("TestUser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var loginDto = new LoginDto { Username = "TestUser", Password = "WrongPassword" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginWithRefreshTokenAsync_ShouldReturnNewTokens_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("TestUser", "test@test.com", "hash") { Id = userId };

        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "old-refresh-token",
            UserId = userId,
            ExpiresOn = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("old-refresh-token"))
            .ReturnsAsync(existingToken);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtProviderMock.Setup(j => j.GenerateAccessToken(user)).Returns("new-access-token");
        _jwtProviderMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");

        _refreshTokenRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginWithRefreshTokenAsync("old-refresh-token");

        // Assert
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(
            It.Is<RefreshToken>(rt => rt.Token == "new-refresh-token")), Times.Once);
    }

    [Fact]
    public async Task LoginWithRefreshTokenAsync_ShouldThrow_WhenTokenNotFound()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("nonexistent-token"))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginWithRefreshTokenAsync("nonexistent-token"));
    }

    [Fact]
    public async Task LoginWithRefreshTokenAsync_ShouldThrow_WhenTokenIsExpired()
    {
        // Arrange
        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired-token",
            UserId = Guid.NewGuid(),
            ExpiresOn = DateTime.UtcNow.AddDays(-1)
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("expired-token"))
            .ReturnsAsync(expiredToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginWithRefreshTokenAsync("expired-token"));
    }

    [Fact]
    public async Task LoginWithRefreshTokenAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-token",
            UserId = Guid.NewGuid(),
            ExpiresOn = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("valid-token"))
            .ReturnsAsync(token);

        _userRepoMock.Setup(r => r.GetByIdAsync(token.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _authService.LoginWithRefreshTokenAsync("valid-token"));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ShouldThrow_WhenIdTokenIsNullOrEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.LoginWithGoogleAsync(""));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.LoginWithGoogleAsync(null!));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ShouldThrow_WhenGoogleTokenIsInvalid()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginWithGoogleAsync("invalid-google-token"));
    }
}