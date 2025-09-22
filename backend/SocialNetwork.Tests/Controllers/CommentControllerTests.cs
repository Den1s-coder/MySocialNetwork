using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;

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

    [Fact]
    public async Task Get_ShouldReturnOk_WhenPostIdFound()
    {
        // Arrange
        var postId = Guid.NewGuid();

        var expectedComments = new List<Comment>
        {
            new Comment { PostId = postId, AuthorId = Guid.NewGuid(), Text = "Test Comment 1" },
            new Comment { PostId = postId, AuthorId = Guid.NewGuid(), Text = "Test Comment 2" }
        };

        _commentServiceMock.Setup(service => service
            .GetPostCommentsAsync(postId))
            .ReturnsAsync(expectedComments);

        // Act
        var result = await _commentController.Get(postId);

        // Assert

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedComments = Assert.IsType<List<Comment>>(okResult.Value);
        Assert.Equal(expectedComments.Count, returnedComments.Count);
    }


}
