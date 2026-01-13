using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet("{postId:guid}/comments")]
        public async Task<IActionResult> GetPaged([FromQuery]int pageNumber, [FromQuery]int pageSize ,Guid postId, CancellationToken cancellationToken = default)
        {
            var comments = await _commentService.GetPostCommentsPagedAsync(postId, pageNumber, pageSize, cancellationToken);

            return Ok(comments);
        }

        [Authorize]
        [HttpPost("CreateComment")]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto createCommentDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreatePostDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            createCommentDto.UserId = UserIdClaim;

            await _commentService.CreateAsync(createCommentDto, cancellationToken);

            return Ok();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> Ban(Guid commentId, CancellationToken cancellationToken = default)
        {
            await _commentService.BanComment(commentId, cancellationToken);

            return Ok();
        }
    }
}
