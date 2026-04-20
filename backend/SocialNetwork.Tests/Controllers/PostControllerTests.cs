using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Comments;
using SocialNetwork.Application.DTO.Posts;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

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

        private void SetupUser(Guid userId, string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAnonymousUser()
        {
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
        }

        [Fact]
        public async Task GetAllPosts_ShouldReturnOk_WithPosts()
        {
            // Arrange
            var expectedPosts = new List<PostDto>
            {
                new PostDto { Id = Guid.NewGuid(), UserName = "User1", Text = "Post 1", IsBanned = false, Comments = new List<CommentDto>() },
                new PostDto { Id = Guid.NewGuid(), UserName = "User2", Text = "Post 2", IsBanned = false, Comments = new List<CommentDto>() }
            };

            _postServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postController.GetAllPosts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<PostDto>>(okResult.Value);
            Assert.Equal(2, returned.Count());
        }

        [Fact]
        public async Task GetAllPosts_ShouldReturnOk_WhenEmpty()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<PostDto>());

            // Act
            var result = await _postController.GetAllPosts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<PostDto>>(okResult.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetPostsPaged_ShouldReturnOk_WithPaginatedResult()
        {
            // Arrange
            int pageNumber = 1, pageSize = 10;

            var expectedResult = new PaginetedResult<PostDto>
            {
                Items = new List<PostDto>
                {
                    new PostDto { Id = Guid.NewGuid(), UserName = "User1", Text = "Post 1", Comments = new List<CommentDto>() }
                },
                TotalCount = 1,
                Page = pageNumber,
                PageSize = pageSize
            };

            _postServiceMock.Setup(s => s.GetPagedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _postController.GetPostsPaged(pageNumber, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PaginetedResult<PostDto>>(okResult.Value);
            Assert.Equal(1, returned.TotalCount);
            Assert.Single(returned.Items);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenPostExists()
        {
            // Arrange
            var postId = Guid.NewGuid();

            var expectedPost = new PostDto
            {
                Id = postId,
                UserName = "TestUser",
                Text = "Test Post",
                IsBanned = false,
                Comments = new List<CommentDto>()
            };

            _postServiceMock.Setup(s => s.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPost);

            // Act
            var result = await _postController.GetById(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPost = Assert.IsType<PostDto>(okResult.Value);
            Assert.Equal(expectedPost.Id, returnedPost.Id);
            Assert.Equal(expectedPost.UserName, returnedPost.UserName);
            Assert.Equal(expectedPost.Text, returnedPost.Text);
        }

        [Fact]
        public async Task GetMyPosts_ShouldReturnOk_WhenUserIsAuthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);

            var expectedPosts = new List<PostDto>
            {
                new PostDto { Id = Guid.NewGuid(), AuthorId = userId, UserName = "Me", Text = "My post", Comments = new List<CommentDto>() }
            };

            _postServiceMock.Setup(s => s.GetPostsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postController.GetMyPosts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<PostDto>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetMyPosts_ShouldReturnUnauthorized_WhenSidClaimMissing()
        {
            // Arrange
            SetupAnonymousUser();

            // Act
            var result = await _postController.GetMyPosts();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetPostsByUserId_ShouldReturnOk_WithUserPosts()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var expectedPosts = new List<PostDto>
            {
                new PostDto { Id = Guid.NewGuid(), AuthorId = userId, UserName = "User1", Text = "Post", Comments = new List<CommentDto>() }
            };

            _postServiceMock.Setup(s => s.GetPostsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postController.GetPostsByUserId(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<PostDto>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WhenModelIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);

            var createDto = new CreatePostDto { Text = "New post" };

            _postServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreatePostDto>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _postController.Create(createDto);

            // Assert
            Assert.IsType<OkResult>(result);
            _postServiceMock.Verify(s => s.CreateAsync(
                It.Is<CreatePostDto>(dto => dto.UserId == userId && dto.Text == "New post"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);

            _postController.ModelState.AddModelError("Text", "Text is required");

            var createDto = new CreatePostDto();

            // Act
            var result = await _postController.Create(createDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _postServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreatePostDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_ShouldReturnUnauthorized_WhenSidClaimMissing()
        {
            // Arrange
            SetupAnonymousUser();

            var createDto = new CreatePostDto { Text = "Post" };

            // Act
            var result = await _postController.Create(createDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        // ---------- ToggleReaction ----------

        [Fact]
        public async Task ToggleReaction_ShouldReturnOk_WhenUserIsAuthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var reactionTypeId = Guid.NewGuid();
            SetupUser(userId);

            var req = new ToggleReactionRequest { ReactionTypeId = reactionTypeId };

            _postServiceMock.Setup(s => s.ToggleReactionAsync(postId, userId, reactionTypeId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _postController.ToggleReaction(postId, req);

            // Assert
            Assert.IsType<OkResult>(result);
            _postServiceMock.Verify(s => s.ToggleReactionAsync(postId, userId, reactionTypeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleReaction_ShouldReturnUnauthorized_WhenSidClaimMissing()
        {
            // Arrange
            SetupAnonymousUser();

            var req = new ToggleReactionRequest { ReactionTypeId = Guid.NewGuid() };

            // Act
            var result = await _postController.ToggleReaction(Guid.NewGuid(), req);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        // ---------- BanPost ----------

        [Fact]
        public async Task BanPost_ShouldReturnOk_WhenPostExists()
        {
            // Arrange
            var postId = Guid.NewGuid();

            _postServiceMock.Setup(s => s.BanPost(postId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _postController.BanPost(postId);

            // Assert
            Assert.IsType<OkResult>(result);
            _postServiceMock.Verify(s => s.BanPost(postId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}