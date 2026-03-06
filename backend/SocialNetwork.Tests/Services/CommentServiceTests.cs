using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.Application.DTO.Comments;
using SocialNetwork.Application.Events;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Mappings;
using SocialNetwork.Application.Service;
using SocialNetwork.Domain.Entities.Comments;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<CommentService>> _loggerMock;
    private readonly Mock<IEventDispatcher> _eventDispatcherMock;
    private readonly IMapper _mapper;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ICommentRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<CommentService>>();
        _eventDispatcherMock = new Mock<IEventDispatcher>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CommentProfile>();
        });
        _mapper = config.CreateMapper();

        _commentService = new CommentService(
            _commentRepoMock.Object,
            _mapper,
            _loggerMock.Object,
            _eventDispatcherMock.Object,
            _userRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedComments()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment("Comment 1", Guid.NewGuid()) { Author = new User("User1", "u1@test.com", "hash") },
            new Comment("Comment 2", Guid.NewGuid()) { Author = new User("User2", "u2@test.com", "hash") }
        };

        _commentRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        // Act
        var result = await _commentService.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnComment_WhenExists()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment("Test", Guid.NewGuid())
        {
            Id = commentId,
            Author = new User("User", "u@test.com", "hash")
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        var result = await _commentService.GetByIdAsync(commentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result!.Text);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment?)null);

        // Act
        var result = await _commentService.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostCommentsPagedAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment("C1", Guid.NewGuid()) { PostId = postId, Author = new User("U1", "u1@test.com", "hash") }
        };

        _commentRepoMock.Setup(r => r.GetPostCommentsPagedAsync(postId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((comments.AsEnumerable(), 1));

        // Act
        var result = await _commentService.GetPostCommentsPagedAsync(postId, 1, 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetPostCommentsPagedAsync_ShouldThrow_WhenPostIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.GetPostCommentsPagedAsync(Guid.Empty, 1, 10));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateComment_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("User", "u@test.com", "hash") { Id = userId, IsBanned = false };

        var createDto = new CreateCommentDto { Text = "New comment", UserId = userId, PostId = postId };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventDispatcherMock.Setup(e => e.DispatchAsync(It.IsAny<CommentCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _commentService.CreateAsync(createDto);

        // Assert
        _commentRepoMock.Verify(r => r.CreateAsync(
            It.Is<Comment>(c => c.Text == "New comment" && c.AuthorId == userId && c.PostId == postId),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcherMock.Verify(e => e.DispatchAsync(It.IsAny<CommentCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDtoIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _commentService.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTextIsEmpty()
    {
        // Arrange
        var createDto = new CreateCommentDto { Text = "   ", UserId = Guid.NewGuid(), PostId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _commentService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTextIsTooLong()
    {
        // Arrange
        var createDto = new CreateCommentDto { Text = new string('x', 1001), UserId = Guid.NewGuid(), PostId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _commentService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var createDto = new CreateCommentDto { Text = "Valid", UserId = Guid.NewGuid(), PostId = Guid.NewGuid() };

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _commentService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserIsBanned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Banned", "b@test.com", "hash") { Id = userId, IsBanned = true };
        var createDto = new CreateCommentDto { Text = "Comment", UserId = userId, PostId = Guid.NewGuid() };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _commentService.CreateAsync(createDto));
    }

    [Fact]
    public async Task BanComment_ShouldSetIsBanned_WhenCommentExists()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment("Test", Guid.NewGuid()) { Id = commentId, IsBanned = false };

        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _commentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _commentService.BanComment(commentId);

        // Assert
        Assert.True(comment.IsBanned);
        _commentRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Comment>(c => c.IsBanned), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BanComment_ShouldThrow_WhenCommentNotFound()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _commentService.BanComment(Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleReactionAsync_ShouldCallRepository_WhenCommentExists()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reactionType = Guid.NewGuid();
        var comment = new Comment("Test", Guid.NewGuid()) { Id = commentId };

        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _commentRepoMock.Setup(r => r.ToggleReactionAsync(commentId, userId, reactionType, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _commentService.ToggleReactionAsync(commentId, userId, reactionType);

        // Assert
        _commentRepoMock.Verify(r => r.ToggleReactionAsync(commentId, userId, reactionType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleReactionAsync_ShouldNotCallRepository_WhenCommentNotFound()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment?)null);

        // Act
        await _commentService.ToggleReactionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        _commentRepoMock.Verify(r => r.ToggleReactionAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}