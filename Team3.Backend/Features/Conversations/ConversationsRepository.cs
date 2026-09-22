using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Conversations.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Conversations;

public sealed class ConversationsRepository : IConversationsRepository
{
    private readonly AppDbContext _context;

    public ConversationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<List<Conversation>> GetForUserAsync(Guid userId)
    {
        return await _context.Conversations
            .AsNoTracking()
            .Include(conversation => conversation.Connection)
            .Include(conversation => conversation.Connection.UserA)
                .ThenInclude(user => user.Profile)
            .Include(conversation => conversation.Connection.UserB)
                .ThenInclude(user => user.Profile)
            .Where(conversation =>
                conversation.Connection.UserAId == userId ||
                conversation.Connection.UserBId == userId)
            .OrderByDescending(conversation => conversation.LastActivityAt)
            .ToListAsync();
    }

    public async Task<Conversation?> GetByIdAsync(Guid conversationId)
    {
        return await _context.Conversations
            .Include(conversation => conversation.Connection)
            .Include(conversation => conversation.Connection.UserA)
                .ThenInclude(user => user.Profile)
            .Include(conversation => conversation.Connection.UserB)
                .ThenInclude(user => user.Profile)
            .FirstOrDefaultAsync(conversation => conversation.Id == conversationId);
    }

    public async Task<Conversation?> GetByIdForUserAsync(
        Guid conversationId,
        Guid userId)
    {
        return await _context.Conversations
            .Include(conversation => conversation.Connection)
            .Include(conversation => conversation.Connection.UserA)
                .ThenInclude(user => user.Profile)
            .Include(conversation => conversation.Connection.UserB)
                .ThenInclude(user => user.Profile)
            .FirstOrDefaultAsync(conversation =>
                conversation.Id == conversationId &&
                (conversation.Connection.UserAId == userId ||
                 conversation.Connection.UserBId == userId));
    }

    public async Task<List<Message>> GetMessagesForConversationAsync(
        Guid conversationId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(message => message.SenderUser)
                .ThenInclude(user => user.Profile)
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.CreatedAt)
            .ToListAsync();
    }

    public async Task<Message?> GetMessageByIdAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(message => message.Conversation)
            .Include(message => message.SenderUser)
                .ThenInclude(user => user.Profile)
            .FirstOrDefaultAsync(message => message.Id == messageId);
    }

    public async Task<Connection?> GetConnectionByIdAsync(Guid connectionId)
    {
        return await _context.Connections
            .Include(connection => connection.UserA)
                .ThenInclude(user => user.Profile)
            .Include(connection => connection.UserB)
                .ThenInclude(user => user.Profile)
            .Include(connection => connection.Conversation)
            .FirstOrDefaultAsync(connection => connection.Id == connectionId);
    }

    public void AddConversation(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
