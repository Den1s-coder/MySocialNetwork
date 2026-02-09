using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO.Auth;
using SocialNetwork.Application.Interfaces;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto createUserDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Register endpoint called");

            await _authService.RegisterAsync(createUserDto, cancellationToken);

            _logger.LogInformation("User Succesfully registered");

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Login endpoint called");

            var responce = await _authService.LoginAsync(loginDto, cancellationToken);

            _logger.LogInformation("User Succesfully login");

            return Ok(responce);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            _logger.LogInformation("Refresh token endpoint called");

            var responce = await _authService.LoginWithRefreshTokenAsync(refreshToken);

            _logger.LogInformation("Token successfully refreshed");

            return Ok(responce);
        }

        [HttpPost("google")]
        public async Task<IActionResult> LoginWithLogin([FromBody] string idToken, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Google login endpoint called");
            var responce = await _authService.LoginWithGoogleAsync(idToken, cancellationToken);
            _logger.LogInformation("User successfully logged in with Google");
            return Ok(responce);
        }
    }
}
