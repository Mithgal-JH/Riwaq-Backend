using FluentAssertions;
using Moq;
using Team3.Backend.Features.Conversations;
using Team3.Backend.Features.Conversations.Dtos;
using Team3.Backend.Features.Conversations.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class ConversationsServiceTests
{
    private readonly Mock<IConversationsRepository> _repository = new();
    private readonly ConversationsService _service;

    public ConversationsServiceTests()
    {
        _service = new ConversationsService(_repository.Object);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldCreateConversationUsingConnectionId_WhenConversationDoesNotExist()
    {
        var currentUser = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());
        var connectionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var connection = new Connection
        {
            Id = connectionId,
            UserAId = currentUser.Id,
            UserBId = otherUser.Id,
            UserA = currentUser,
            UserB = otherUser,
            CreatedAt = DateTime.UtcNow
        };

        _repository.Setup(x => x.GetByIdAsync(conversationId))
            .ReturnsAsync((Conversation?)null);
        _repository.Setup(x => x.GetConnectionByIdAsync(connectionId))
            .ReturnsAsync(connection);
        _repository.Setup(x => x.GetUserByIdAsync(currentUser.Id))
            .ReturnsAsync(currentUser);

        var request = new SendMessageRequest
        {
            ConnectionId = connectionId,
            Subject = "Question about C#",
            Content = "Hi, I wanted to ask about..."
        };

        var response = await _service.SendMessageAsync(currentUser.Id, conversationId, request);

        response.Content.Should().Be("Hi, I wanted to ask about...");
        response.Sender.UserId.Should().Be(currentUser.Id);

        _repository.Verify(x => x.AddConversation(It.Is<Conversation>(conversation =>
            conversation.Id == conversationId &&
            conversation.ConnectionId == connectionId &&
            conversation.Subject == "Question about C#")), Times.Once);
        _repository.Verify(x => x.AddMessage(It.Is<Message>(message =>
            message.ConversationId == conversationId &&
            message.SenderUserId == currentUser.Id &&
            message.Content == "Hi, I wanted to ask about...")), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldRequireConnectionId_WhenConversationDoesNotExist()
    {
        var currentUser = ServiceTestData.User(Guid.NewGuid());
        var conversationId = Guid.NewGuid();

        _repository.Setup(x => x.GetByIdAsync(conversationId))
            .ReturnsAsync((Conversation?)null);

        var request = new SendMessageRequest
        {
            Subject = "Question about C#",
            Content = "Hi, I wanted to ask about..."
        };

        var action = () => _service.SendMessageAsync(currentUser.Id, conversationId, request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("ConnectionId is required when creating the first conversation message.");
    }
}
