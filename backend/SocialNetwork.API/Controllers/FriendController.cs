using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly ILogger<FriendController> _logger;
        private readonly IFriendService _friendService;
        public FriendController(ILogger<FriendController> logger, IFriendService friendService)
        {
            _logger = logger;
            _friendService = friendService;
        }

        [HttpGet("GetAllFriends")]
        public async Task<IActionResult> GetAllFriends(CancellationToken cancellationToken = default)
        {
            var friends = await _friendService.GetAllFriends(cancellationToken);
            return Ok(friends);
        }

        [HttpGet("{userId}/Friends")]
        public async Task<IActionResult> GetFriendsOfUser(Guid userId, CancellationToken cancellationToken = default)
        {
            var friends = await _friendService.GetFriendsOfUser(userId, cancellationToken);
            return Ok(friends);
        }

        [Authorize]
        [HttpGet("PendingRequests")]
        public async Task<IActionResult> GetPendingFriendRequests(CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            if (UserIdClaim == Guid.Empty)
            {
                _logger.LogWarning("UserId claim is missing or invalid.");
                return Unauthorized();
            }

            var requests = await _friendService.GetPendingFriendRequests(UserIdClaim, cancellationToken);
            return Ok(requests);
        }

        [Authorize]
        [HttpGet("MyFriends")]
        public async Task<IActionResult> GetMyFriends(CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            if (UserIdClaim == Guid.Empty)
            {
                _logger.LogWarning("UserId claim is missing or invalid.");
                return Unauthorized();
            }
            var friends = await _friendService.GetFriendsOfUser(UserIdClaim, cancellationToken);
            return Ok(friends);
        }

        [Authorize]
        [HttpPost("SendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest([FromBody] Guid addresseeId, CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            if (UserIdClaim == Guid.Empty)
            {
                _logger.LogWarning("UserId claim is missing or invalid.");
                return Unauthorized();
            }

            var request = new FriendRequestDto
            {
                RequesterId = UserIdClaim,
                ReceiverId = addresseeId,
                RequestedAt = DateTime.UtcNow
            };

            await _friendService.SendFriendRequest(request, cancellationToken);
            return Ok();

        }

        [Authorize]
        [HttpPost("AcceptFriendRequest/{requestId:guid}")]
        public async Task<IActionResult> AcceptFriendRequest(Guid requestId, CancellationToken cancellationToken = default)
        {
            await _friendService.AcceptFriendRequest(requestId, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("DeclineFriendRequest/{requestId:guid}")]
        public async Task<IActionResult> DeclineFriendRequest(Guid requestId, CancellationToken cancellationToken = default)
        {
            await _friendService.DeclineFriendRequest(requestId, cancellationToken);
            return Ok();
        }

        [Authorize]
        [HttpDelete("RemoveFriend/{friendId:guid}")]
        public async Task<IActionResult> RemoveFriend(Guid friendId, CancellationToken cancellationToken = default)
        {
            var UserIdClaim = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            if (UserIdClaim == Guid.Empty)
            {
                _logger.LogWarning("UserId claim is missing or invalid.");
                return Unauthorized();
            }

            await _friendService.RemoveFriend(UserIdClaim, friendId);
            return Ok();
        }
    }
}
