using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(CancellationToken cancellationToken)
        {
            try
            {
                var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                if (!Guid.TryParse(sid, out var userId))
                    return Unauthorized();

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving notifications.");
            }
        }

        [Authorize]
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications(CancellationToken cancellationToken)
        {
            try
            {
                var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                if (!Guid.TryParse(sid, out var userId))
                    return Unauthorized();

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId, cancellationToken);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving unread notifications.");
            }
        }

        [Authorize]
        [HttpPost("markread/{notificationId:guid}")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
        {
            try
            {
                var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                if (!Guid.TryParse(sid, out var userId))
                    return Unauthorized();

                await _notificationService.MarkAsReadAsync(userId, notificationId, cancellationToken);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Notification not found: {NotificationId}", notificationId);
                return NotFound("Notification not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while marking the notification as read.");
            }
        }

        [Authorize]
        [HttpPost("markreadall")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            try
            {
                var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                if (!Guid.TryParse(sid, out var userId))
                    return Unauthorized();
                await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while marking all notifications as read.");
            }
        }
    }
}
