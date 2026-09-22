using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Comments.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Comments;

public class CommentsRepository : ICommentsRepository
{
    private readonly AppDbContext _context;

    // Store the database context used by this repository.
    public CommentsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        // Find the application user using Firebase UID.
        return await _context.Users
            .FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid);
    }
    // Check whether educational content exists.
    public async Task<bool> EducationalContentExistsAsync(
        Guid educationalContentId)
    {
        return await _context.EducationalContents
            .AnyAsync(content => content.Id == educationalContentId);
    }

    public async Task<Guid?> GetEducationalContentOwnerIdAsync(
        Guid educationalContentId)
    {
        return await _context.EducationalContents
            .Where(content => content.Id == educationalContentId)
            .Select(content => (Guid?)content.UserId)
            .FirstOrDefaultAsync();
    }
    public async Task<List<Comment>> GetByContentIdAsync(Guid educationalContentId)
    {
        // Get comments for the requested educational content.
        return await _context.Comments
            .AsNoTracking()
            .Where(comment => comment.EducationalContentId == educationalContentId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        // Find a comment by its ID.
        return await _context.Comments
            .FirstOrDefaultAsync(comment => comment.Id == commentId);
    }
    public async Task<Comment?> GetByIdForContentAsync(
        Guid commentId,
        Guid educationalContentId)
    {
        // Find the parent comment only if it belongs to the same content.
        return await _context.Comments
            .FirstOrDefaultAsync(comment =>
                comment.Id == commentId
                && comment.EducationalContentId == educationalContentId);

    }
    public void Add(Comment comment)
    {
        // Add the new comment to the database context.
        _context.Comments.Add(comment);
    }

    public void Remove(Comment comment)
    {
        // Mark the comment for deletion.
        _context.Comments.Remove(comment);
    }

    public async Task SaveChangesAsync()
    {
        // Save all pending changes to the database.
        await _context.SaveChangesAsync();
    }
}