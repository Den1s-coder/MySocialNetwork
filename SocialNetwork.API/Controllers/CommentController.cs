using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{postId:guid}/comments")]
        public async Task<IActionResult> Get(Guid postId)
        {
            IEnumerable<Comment> comments = await _commentService.GetPostCommentsAsync(postId);

            return Ok(comments);
        }

        [HttpPost("CreateComment")]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _commentService.CreateAsync(createCommentDto);

            return Ok();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> Ban(Guid commentId)
        {
            await _commentService.BanComment(commentId);

            return Ok();
        }
    }
}
