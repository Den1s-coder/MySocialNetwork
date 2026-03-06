using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Controllers;
using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<ILogger<ChatController>> _loggerMock;
    private readonly Mock<IChatService> _chatServiceMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly ChatController _chatController;

    public ChatControllerTests()
    {
        _loggerMock = new Mock<ILogger<ChatController>>();
        _chatServiceMock = new Mock<IChatService>();
        _messageServiceMock = new Mock<IMessageService>();

        _chatController = new ChatController(
            _loggerMock.Object,
            _messageServiceMock.Object,
            _chatServiceMock.Object);
    }

    private void SetupUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, userId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _chatController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupAnonymousUser()
    {
        _chatController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task MyChats_ShouldReturnOk_WithChats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var expectedChats = new List<ChatDto>
        {
            new ChatDto { Id = Guid.NewGuid(), Title = "Chat 1" },
            new ChatDto { Id = Guid.NewGuid(), Title = "Chat 2" }
        };

        _chatServiceMock.Setup(s => s.GetChatsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedChats);

        // Act
        var result = await _chatController.MyChats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<ChatDto>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task MyChats_ShouldReturnOk_WhenNoChats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        _chatServiceMock.Setup(s => s.GetChatsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ChatDto>());

        // Act
        var result = await _chatController.MyChats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<ChatDto>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnOk_WithMessages()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var chatId = Guid.NewGuid();
        var expectedMessages = new List<MessageDto>
        {
            new MessageDto { Id = Guid.NewGuid(), ChatId = chatId, SenderId = userId, Content = "Hello" },
            new MessageDto { Id = Guid.NewGuid(), ChatId = chatId, SenderId = Guid.NewGuid(), Content = "Hi" }
        };

        _messageServiceMock.Setup(s => s.GetMessageByChatIdAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _chatController.GetMessages(chatId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<MessageDto>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetMessages_ShouldReturnOk_WhenNoMessages()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        var chatId = Guid.NewGuid();

        _messageServiceMock.Setup(s => s.GetMessageByChatIdAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<MessageDto>());

        // Act
        var result = await _chatController.GetMessages(chatId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<MessageDto>>(okResult.Value);
        Assert.Empty(returned);
    }

    [Fact]
    public async Task CreatePrivateChat_ShouldReturnOk_WithChatId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        SetupUser(userId);

        var expectedChat = new ChatDto { Id = chatId, Title = "Private" };

        _chatServiceMock.Setup(s => s.CreatePrivateChatAsync(userId, secondUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedChat);

        // Act
        var result = await _chatController.CreatePrivateChat(secondUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var chatIdProp = okResult.Value!.GetType().GetProperty("chatId");
        Assert.NotNull(chatIdProp);
        Assert.Equal(chatId, (Guid)chatIdProp!.GetValue(okResult.Value)!);
    }

    [Fact]
    public async Task ToggleReaction_ShouldReturnNoContent_WhenUserIsAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var reactionType = Guid.NewGuid();
        SetupUser(userId);

        _messageServiceMock.Setup(s => s.ToogleReactionAsync(messageId, userId, reactionType, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _chatController.ToggleReaction(messageId, reactionType);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _messageServiceMock.Verify(s => s.ToogleReactionAsync(messageId, userId, reactionType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleReaction_ShouldReturnUnauthorized_WhenSidClaimMissing()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _chatController.ToggleReaction(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}