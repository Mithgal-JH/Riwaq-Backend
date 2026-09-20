using Team3.Backend.Models;

namespace Team3.Backend.Features.Conversations.Interfaces;

public interface IConversationsRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<List<Conversation>> GetForUserAsync(Guid userId);

    Task<Conversation?> GetByIdAsync(Guid conversationId);

    Task<Conversation?> GetByIdForUserAsync(Guid conversationId, Guid userId);

    Task<List<Message>> GetMessagesForConversationAsync(Guid conversationId);

    Task<Message?> GetMessageByIdAsync(Guid messageId);

    Task<Connection?> GetConnectionByIdAsync(Guid connectionId);

    void AddConversation(Conversation conversation);

    void AddMessage(Message message);

    Task SaveChangesAsync();
}
