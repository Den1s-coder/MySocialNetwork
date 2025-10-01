using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Service;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
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

        [Authorize]
        [Route("profile")]
        [HttpGet]
        public async Task<IActionResult> GetMyPosts()
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            var posts = await _postService.GetPostsByUserIdAsync(UserIdClaim);
            return Ok(posts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreatePostDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            createPostDto.UserId = UserIdClaim;

            await _postService.CreateAsync(createPostDto);

            return Ok();
        }

        [Authorize]
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
