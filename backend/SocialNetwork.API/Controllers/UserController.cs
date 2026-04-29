using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetAllAsync(cancellationToken);
            return Ok(users);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginetedResult<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginetedResult<UserDto>>> SearchUsers(
            [FromQuery] string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var result = await _userService.SearchAsync(query, pageNumber, pageSize, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            return Ok(user);
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            return Ok(user);
        }

        [HttpGet("by-name/{userName}")]
        public async Task<IActionResult> GetUserByName(string userName, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetByUserNameAsync(userName, cancellationToken);
            return Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto userDto, CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            await _userService.UpdateAsync(userId, userDto, cancellationToken);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId:guid}/ban")]
        public async Task<IActionResult> BanUser(Guid userId, CancellationToken cancellationToken = default)
        {
            await _userService.BanUserAsync(userId, cancellationToken);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId:guid}/role")]
        public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeUserRoleRequest request, CancellationToken cancellationToken = default)
        {
            await _userService.ChangeRoleAsync(userId, request.NewRole, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            await _userService.ChangePasswordAsync(userId, changePasswordDto, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto changeEmailDto, CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            await _userService.ChangeEmailAsync(userId, changeEmailDto, cancellationToken);
            return Ok();
        }
    }
}
