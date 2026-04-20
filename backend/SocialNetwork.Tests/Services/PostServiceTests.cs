using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.Application.DTO.Posts;
using SocialNetwork.Application.Events;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Mappings;
using SocialNetwork.Application.Service;
using SocialNetwork.Domain.Entities.Posts;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Tests.Services;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _postRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<PostService>> _loggerMock;
    private readonly Mock<IEventDispatcher> _eventDispatcherMock;
    private readonly IMapper _mapper;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        _postRepoMock = new Mock<IPostRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<PostService>>();
        _eventDispatcherMock = new Mock<IEventDispatcher>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PostProfile>();
            cfg.AddProfile<CommentProfile>();
        });
        _mapper = config.CreateMapper();

        _postService = new PostService(
            _postRepoMock.Object,
            _userRepoMock.Object,
            _mapper,
            _loggerMock.Object,
            _eventDispatcherMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new Post("Post 1", Guid.NewGuid()) { User = new User("User1", "u1@test.com", "hash") },
            new Post("Post 2", Guid.NewGuid()) { User = new User("User2", "u2@test.com", "hash") }
        };

        _postRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(posts);

        // Act
        var result = await _postService.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoPosts()
    {
        // Arrange
        _postRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Post>());

        // Act
        var result = await _postService.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPost_WhenExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post("Test", Guid.NewGuid()) { Id = postId, User = new User("User", "u@test.com", "hash") };

        _postRepoMock.Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        // Act
        var result = await _postService.GetByIdAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result!.Text);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _postRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var posts = new List<Post>
        {
            new Post("Post 1", Guid.NewGuid()) { User = new User("User1", "u1@test.com", "hash") }
        };

        _postRepoMock.Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((posts.AsEnumerable(), 1));

        // Act
        var result = await _postService.GetPagedAsync(1, 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetPostsByUserIdAsync_ShouldReturnUserPosts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var posts = new List<Post>
        {
            new Post("My Post", userId) { User = new User("Me", "me@test.com", "hash") }
        };

        _postRepoMock.Setup(r => r.GetPostsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(posts);

        // Act
        var result = await _postService.GetPostsByUserIdAsync(userId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePost_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("User", "u@test.com", "hash") { Id = userId, IsBanned = false };
        var createDto = new CreatePostDto { Text = "New post", UserId = userId };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _postRepoMock.Setup(r => r.CreateAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventDispatcherMock.Setup(e => e.DispatchAsync(It.IsAny<PostCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _postService.CreateAsync(createDto);

        // Assert
        _postRepoMock.Verify(r => r.CreateAsync(
            It.Is<Post>(p => p.Text == "New post" && p.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcherMock.Verify(e => e.DispatchAsync(It.IsAny<PostCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDtoIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _postService.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTextIsEmpty()
    {
        // Arrange
        var createDto = new CreatePostDto { Text = "   ", UserId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _postService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTextIsTooLong()
    {
        // Arrange
        var createDto = new CreatePostDto { Text = new string('x', 5001), UserId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _postService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var createDto = new CreatePostDto { Text = "Valid text", UserId = Guid.NewGuid() };

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _postService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserIsBanned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Banned", "b@test.com", "hash") { Id = userId, IsBanned = true };
        var createDto = new CreatePostDto { Text = "Post", UserId = userId };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _postService.CreateAsync(createDto));
    }

    [Fact]
    public async Task BanPost_ShouldSetIsBanned_WhenPostExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post("Test", Guid.NewGuid()) { Id = postId, IsBanned = false };

        _postRepoMock.Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _postRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _postService.BanPost(postId);

        // Assert
        Assert.True(post.IsBanned);
        _postRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Post>(p => p.IsBanned), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BanPost_ShouldThrow_WhenPostNotFound()
    {
        // Arrange
        _postRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _postService.BanPost(Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleReactionAsync_ShouldCallRepository_WhenPostExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reactionType = Guid.NewGuid();
        var post = new Post("Test", Guid.NewGuid()) { Id = postId };

        _postRepoMock.Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _postRepoMock.Setup(r => r.ToggleReactionAsync(postId, userId, reactionType, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _postService.ToggleReactionAsync(postId, userId, reactionType);

        // Assert
        _postRepoMock.Verify(r => r.ToggleReactionAsync(postId, userId, reactionType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleReactionAsync_ShouldThrow_WhenPostNotFound()
    {
        // Arrange
        _postRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.ToggleReactionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }
}