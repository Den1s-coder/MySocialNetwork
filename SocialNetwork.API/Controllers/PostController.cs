using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Service;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

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
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _postService.GetAllAsync();

            return Ok(posts);
        }

        [HttpGet("{postId:guid}")]
        public async Task<IActionResult> GetById(Guid postId)
        {
            var post = await _postService.GetByIdAsync(postId);

            return Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreatePostDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            await _postService.CreateAsync(createPostDto);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] string value)//TODO: Realise Update post and aa DTO for Update
        {

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{postId:guid}")]
        public async Task<IActionResult> BanPost(Guid postId)
        {
            await _postService.BanPost(postId);

            return Ok();
        }
    }
}
