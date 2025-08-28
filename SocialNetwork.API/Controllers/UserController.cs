using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Interfaces;

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
    }
}
