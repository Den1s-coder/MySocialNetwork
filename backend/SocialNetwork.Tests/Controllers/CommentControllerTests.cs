using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Comments;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.Tests.Controllers;

public class CommentControllerTests
{
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly Mock<ILogger<CommentController>> _loggerMock;
    private readonly CommentController _commentController;

    public CommentControllerTests()
    {
        _commentServiceMock = new Mock<ICommentService>();
        _loggerMock = new Mock<ILogger<CommentController>>();

        _commentController = new CommentController(_commentServiceMock.Object, _loggerMock.Object);
    }

    private void SetupUser(Guid userId, string role = "User")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _commentController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WithPaginatedComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        int pageNumber = 1, pageSize = 10;

        var expectedResult = new PaginetedResult<CommentDto>
        {
            Items = new List<CommentDto>
            {
                new CommentDto { Id = Guid.NewGuid(), UserName = "User1", Text = "Comment 1" },
                new CommentDto { Id = Guid.NewGuid(), UserName = "User2", Text = "Comment 2" }
            },
            TotalCount = 2,
            Page = pageNumber,
            PageSize = pageSize
        };

        _commentServiceMock.Setup(s => s.GetPostCommentsPagedAsync(postId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _commentController.GetPaged(pageNumber, pageSize, postId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<PaginetedResult<CommentDto>>(okResult.Value);
        Assert.Equal(expectedResult.TotalCount, returned.TotalCount);
        Assert.Equal(expectedResult.Items.Count(), returned.Items.Count());
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenNoComments()
    {
        // Arrange
        var postId = Guid.NewGuid();

        var expectedResult = new PaginetedResult<CommentDto>
        {
            Items = Enumerable.Empty<CommentDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _commentServiceMock.Setup(s => s.GetPostCommentsPagedAsync(postId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _commentController.GetPaged(1, 10, postId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<PaginetedResult<CommentDto>>(okResult.Value);
        Assert.Empty(returned.Items);
        Assert.Equal(0, returned.TotalCount);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenModelIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var createDto = new CreateCommentDto
        {
            Text = "New comment",
            PostId = Guid.NewGuid()
        };

        _commentServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateCommentDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _commentController.Create(createDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _commentServiceMock.Verify(s => s.CreateAsync(
            It.Is<CreateCommentDto>(dto => dto.UserId == userId && dto.Text == "New comment"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _commentController.ModelState.AddModelError("Text", "Text is required");

        var createDto = new CreateCommentDto { PostId = Guid.NewGuid() };

        // Act
        var result = await _commentController.Create(createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _commentServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreateCommentDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ToggleReaction_ShouldReturnOk_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var reactionType = Guid.NewGuid();
        SetupUser(userId);

        _commentServiceMock.Setup(s => s.ToggleReactionAsync(commentId, userId, reactionType, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _commentController.ToggleReaction(commentId, reactionType);

        // Assert
        Assert.IsType<OkResult>(result);
        _commentServiceMock.Verify(s => s.ToggleReactionAsync(commentId, userId, reactionType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleReaction_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        _commentController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act
        var result = await _commentController.ToggleReaction(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Ban_ShouldReturnOk_WhenCommentExists()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        _commentServiceMock.Setup(s => s.BanComment(commentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _commentController.Ban(commentId);

        // Assert
        Assert.IsType<OkResult>(result);
        _commentServiceMock.Verify(s => s.BanComment(commentId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
