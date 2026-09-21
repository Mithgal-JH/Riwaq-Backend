using Team3.Backend.Features.Conversations.Dtos;
using Team3.Backend.Features.Conversations.Interfaces;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Conversations;

public sealed class ConversationsService : IConversationsService
{
    private readonly IConversationsRepository _repository;

    public ConversationsService(IConversationsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(
        Guid userId)
    {
        var conversations = await _repository.GetForUserAsync(userId);

        return conversations
            .Select(MapConversation)
            .ToList();
    }

    public async Task<ConversationResponse?> GetConversationByIdAsync(
        Guid userId,
        Guid conversationId)
    {
        var conversation = await _repository.GetByIdForUserAsync(
            conversationId,
            userId);

        return conversation is null
            ? null
            : MapConversation(conversation);
    }

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid userId,
        Guid conversationId)
    {
        var conversation = await _repository.GetByIdForUserAsync(
            conversationId,
            userId);

        if (conversation is null)
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        var messages = await _repository.GetMessagesForConversationAsync(
            conversationId);

        return messages
            .Select(MapMessage)
            .ToList();
    }

    public async Task<MessageResponse> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        SendMessageRequest request)
    {
        if (request is null)
        {
            throw new ArgumentException("Request body is required.");
        }

        var trimmedContent = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedContent))
        {
            throw new ArgumentException("Content is required.");
        }

        var now = DateTime.UtcNow;

        var conversation = await _repository.GetByIdAsync(conversationId);

        if (conversation is null)
        {
            if (request.ConnectionId is null || request.ConnectionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "ConnectionId is required when creating the first conversation message.");
            }

            var connection = await _repository.GetConnectionByIdAsync(
                request.ConnectionId.Value);

            if (connection is null)
            {
                throw new KeyNotFoundException("Connection not found.");
            }

            if (connection.UserAId != userId && connection.UserBId != userId)
            {
                throw new InvalidOperationException(
                    "You do not belong to this connection.");
            }

            if (connection.Conversation is not null)
            {
                throw new InvalidOperationException(
                    "This connection already has a conversation.");
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new ArgumentException(
                    "Subject is required when creating the first conversation message.");
            }

            conversation = new Conversation
            {
                Id = conversationId,
                ConnectionId = connection.Id,
                Subject = request.Subject.Trim(),
                CreatedAt = now,
                LastActivityAt = now,
                Connection = connection
            };

            _repository.AddConversation(conversation);
        }
        else if (conversation.Connection.UserAId != userId &&
                 conversation.Connection.UserBId != userId)
        {
            throw new InvalidOperationException(
                "You do not have access to this conversation.");
        }

        if (conversation.Subject is null &&
            !string.IsNullOrWhiteSpace(request.Subject))
        {
            conversation.Subject = request.Subject.Trim();
        }

        var sender = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderUserId = userId,
            Content = trimmedContent,
            CreatedAt = now,
            UpdatedAt = now,
            Conversation = conversation,
            SenderUser = sender
        };

        conversation.LastActivityAt = now;
        _repository.AddMessage(message);
        await _repository.SaveChangesAsync();

        return MapMessage(message);
    }

    public async Task<MessageResponse> UpdateMessageAsync(
        Guid userId,
        Guid messageId,
        UpdateMessageRequest request)
    {
        if (request is null)
        {
            throw new ArgumentException("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required.");
        }

        var message = await _repository.GetMessageByIdAsync(messageId);

        if (message is null)
        {
            throw new KeyNotFoundException("Message not found.");
        }

        if (message.SenderUserId != userId)
        {
            throw new InvalidOperationException(
                "You can only update your own message.");
        }

        message.Content = request.Content.Trim();
        message.UpdatedAt = DateTime.UtcNow;

        if (message.Conversation is not null)
        {
            message.Conversation.LastActivityAt = message.UpdatedAt;
        }

        await _repository.SaveChangesAsync();

        return MapMessage(message);
    }

    private static ConversationResponse MapConversation(Conversation conversation)
    {
        var participants = new List<PublicUserProfileResponse>
        {
            MapProfile(conversation.Connection.UserA),
            MapProfile(conversation.Connection.UserB)
        };

        return new ConversationResponse
        {
            Id = conversation.Id,
            Subject = conversation.Subject,
            Participants = participants,
            LastActivityAt = conversation.LastActivityAt
        };
    }

    private static MessageResponse MapMessage(Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            Sender = MapProfile(message.SenderUser),
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }

    private static PublicUserProfileResponse MapProfile(User user)
    {
        return new PublicUserProfileResponse
        {
            UserId = user.Id,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            LearningDirectionName = user.SelectedSkill?.Name,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }
}
