using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Tests.Controllers
{
    public class PostControllerTests
    {
        private readonly Mock<IPostService> _postServiceMock;
        private readonly Mock<ILogger<PostController>> _loggerMock;
        private readonly PostController _postController;

        public PostControllerTests()
        {
            _postServiceMock = new Mock<IPostService>();
            _loggerMock = new Mock<ILogger<PostController>>();

            _postController = new PostController(_postServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WhenUserIdFound()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var expectedPost = new PostDto
            {
                Id = postId, UserName= "",Text = "Test Post" , IsBanned = false, Comments = new List<CommentDto>()
            };

            _postServiceMock.Setup(service => service
                .GetByIdAsync(postId))
                .ReturnsAsync(expectedPost);

            // Act
            var result = await _postController.GetById(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPost = Assert.IsType<PostDto>(okResult.Value);
            Assert.Equal(expectedPost.Id, returnedPost.Id);
            Assert.Equal(expectedPost.UserName, returnedPost.UserName);
        }
    }
}