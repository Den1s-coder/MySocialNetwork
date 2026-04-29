using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Posts;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostController> _logger;

        public PostController(IPostService postService, ILogger<PostController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts(CancellationToken cancellationToken = default)
        {
            var posts = await _postService.GetAllAsync(cancellationToken);
            return Ok(posts);
        }

        [HttpGet("posts")]
        public async Task<IActionResult> GetPostsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedPosts = await _postService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
            return Ok(pagedPosts);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginetedResult<PostDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginetedResult<PostDto>>> SearchPosts(
            [FromQuery] string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var result = await _postService.SearchAsync(query, pageNumber, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{postId:guid}")]
        public async Task<IActionResult> GetById(Guid postId, CancellationToken cancellationToken = default)
        {
            var post = await _postService.GetByIdAsync(postId, cancellationToken);
            return Ok(post);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPosts()
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            var posts = await _postService.GetPostsByUserIdAsync(userId);
            return Ok(posts);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetPostsByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId, cancellationToken);
            return Ok(posts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto createPostDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreatePostDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            createPostDto.UserId = userId;
            await _postService.CreateAsync(createPostDto, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("{postId:guid}/react")]
        public async Task<IActionResult> ToggleReaction(Guid postId, [FromQuery] ToggleReactionRequest req, CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            await _postService.ToggleReactionAsync(postId, userId, req.ReactionTypeId, cancellationToken);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{postId:guid}")]
        public async Task<IActionResult> BanPost(Guid postId, CancellationToken cancellationToken = default)
        {
            await _postService.BanPost(postId, cancellationToken);
            return Ok();
        }
    }
}
