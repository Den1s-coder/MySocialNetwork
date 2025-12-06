using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var usersDto = await _userService.GetAllUsersAsync();

            return Ok(usersDto);
        }
        
        [HttpGet("users/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userDto = await _userService.GetByIdAsync(id);

            if (userDto == null) return NotFound();

            return Ok(userDto);
        }

        [HttpGet("users/by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var userDto = await _userService.GetUserByEmailAsync(email);
            if (userDto == null)
            {
                return NotFound();
            }
            return Ok(userDto);
        }

        [HttpGet("users/by-username")]
        public async Task<IActionResult> GetByUserName([FromQuery] string username)
        {
            var userDto = await _userService.GetUserByNameAsync(username);

            if (userDto == null) return NotFound();
            
            return Ok(userDto);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (string.IsNullOrEmpty(sid) || !Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning("GetProfile: missing or invalid Sid claim");
                return Unauthorized();
            }

            var userDto = await _userService.GetByIdAsync(userId);
            if (userDto == null)
            {
                _logger.LogWarning("GetProfile: user not found {UserId}", userId);
                return NotFound();
            }

            return Ok(userDto);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto updatedUserDto)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            
            updatedUserDto.Id = UserIdClaim;

            await _userService.UpdateProfileAsync(updatedUserDto);
            return Ok();
        }
    }
}
