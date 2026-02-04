using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Application.DTO.Users;
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
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken = default)
        {
            var usersDto = await _userService.GetAllUsersAsync(cancellationToken);

            return Ok(usersDto);
        }

        [HttpGet("users/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var userDto = await _userService.GetByIdAsync(id, cancellationToken);

            if (userDto == null) return NotFound();

            return Ok(userDto);
        }

        [HttpGet("users/by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken cancellationToken = default)
        {
            var userDto = await _userService.GetUserByEmailAsync(email, cancellationToken);
            if (userDto == null)
            {
                return NotFound();
            }
            return Ok(userDto);
        }

        [HttpGet("users/by-username")]
        public async Task<IActionResult> GetByUserName([FromQuery] string username, CancellationToken cancellationToken = default)
        {
            var userDto = await _userService.GetUserByNameAsync(username, cancellationToken);

            if (userDto == null) return NotFound();

            return Ok(userDto);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (string.IsNullOrEmpty(sid) || !Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning("GetProfile: missing or invalid Sid claim");
                return Unauthorized();
            }

            var userDto = await _userService.GetByIdAsync(userId, cancellationToken);
            if (userDto == null)
            {
                _logger.LogWarning("GetProfile: user not found {UserId}", userId);
                return NotFound();
            }

            return Ok(userDto);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto updatedUserDto, CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            updatedUserDto.Id = UserIdClaim;

            await _userService.UpdateProfileAsync(updatedUserDto, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            await _userService.ChangePasswordAsync(UserIdClaim, changePasswordDto, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto changeEmailDto, CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            await _userService.ChangeEmailAsync(UserIdClaim, changeEmailDto, cancellationToken);
            return Ok();
        }
    }
}
