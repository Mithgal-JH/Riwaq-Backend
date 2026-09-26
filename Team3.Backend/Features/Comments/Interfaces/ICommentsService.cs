using Team3.Backend.Features.Comments.DTOs;

namespace Team3.Backend.Features.Comments.Interfaces;

public interface ICommentsService
{
    // Get all comments for an educational content.
    Task<List<CommentResponse>> GetByContentIdAsync(
        Guid educationalContentId);

    // Create a new comment or reply.
    Task<CommentResponse> CreateAsync(
        string firebaseUid,
        Guid educationalContentId,
        CreateCommentRequest request);

    // Update an existing comment.
    Task<CommentResponse> UpdateAsync(
        string firebaseUid,
        Guid commentId,
        UpdateCommentRequest request);

    // Delete an existing comment.
    Task DeleteAsync(
        string firebaseUid,
        Guid commentId);
}