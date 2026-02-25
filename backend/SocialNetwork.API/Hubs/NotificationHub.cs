using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SocialNetwork.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
