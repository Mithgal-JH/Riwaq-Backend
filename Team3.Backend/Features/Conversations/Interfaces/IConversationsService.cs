using Team3.Backend.Features.Conversations.Dtos;

namespace Team3.Backend.Features.Conversations.Interfaces;

public interface IConversationsService
{
    Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(
        Guid userId);

    Task<ConversationResponse?> GetConversationByIdAsync(
        Guid userId,
        Guid conversationId);

    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid userId,
        Guid conversationId);

    Task<MessageResponse> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        SendMessageRequest request);

    Task<MessageResponse> UpdateMessageAsync(
        Guid userId,
        Guid messageId,
        UpdateMessageRequest request);
}
