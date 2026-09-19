using Team3.Backend.Models;

namespace Team3.Backend.Features.Comments.Interfaces;

public interface ICommentsRepository
{
    // Get the user by Firebase UID.
    Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);
    // Check whether educational content exists.
    Task<bool> EducationalContentExistsAsync(Guid educationalContentId);
    // Get all comments for a specific educational content.
    Task<List<Comment>> GetByContentIdAsync(Guid educationalContentId);

    // Get a specific comment by its ID.
    Task<Comment?> GetByIdAsync(Guid commentId);

    // Get a parent comment that belongs to the same educational content.
    Task<Comment?> GetByIdForContentAsync(
        Guid commentId,
        Guid educationalContentId);
    // Add a new comment.
    void Add(Comment comment);

    // Remove an existing comment.
    void Remove(Comment comment);

    // Save changes to the database.
    Task SaveChangesAsync();
}