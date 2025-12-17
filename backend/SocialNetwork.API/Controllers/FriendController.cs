using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Interfaces;

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
    }
}
