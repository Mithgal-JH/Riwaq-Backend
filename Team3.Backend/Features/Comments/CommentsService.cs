using Team3.Backend.Features.Comments.DTOs;
using Team3.Backend.Features.Comments.Interfaces;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Comments;

public class CommentsService : ICommentsService
{
    private readonly ICommentsRepository _repository;
    private readonly INotificationsService _notificationsService;

    // Store the repository used to access comment data.
    public CommentsService(
        ICommentsRepository repository,
        INotificationsService notificationsService)
    {
        _repository = repository;
        _notificationsService = notificationsService;
    }

    public async Task<List<CommentResponse>> GetByContentIdAsync(
        Guid educationalContentId)
    {
        // Validate the educational content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException("Educational content ID is required.");
        }
        // Check that the educational content exists.
        var contentExists = await _repository
            .EducationalContentExistsAsync(educationalContentId);

        if (!contentExists)
        {
            throw new KeyNotFoundException(
                "Educational content not found.");
        }
        var comments = await _repository.GetByContentIdAsync(
            educationalContentId);

        // Map comment entities to response DTOs.
        return comments.Select(MapToResponse).ToList();
    }

    public async Task<CommentResponse> CreateAsync(
        string firebaseUid,
        Guid educationalContentId,
        CreateCommentRequest request)
    {
        // Validate the educational content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException("Educational content ID is required.");
        }
        // Check that the educational content exists.
        var contentExists = await _repository
            .EducationalContentExistsAsync(educationalContentId);

        if (!contentExists)
        {
            throw new KeyNotFoundException(
                "Educational content not found.");
        }
        // Validate the comment content.
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Comment content is required.");
        }

        var user = await _repository.GetUserByFirebaseUidAsync(firebaseUid);

        // Make sure the Firebase user exists in the application database.
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Validate the parent comment when this is a reply.
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _repository.GetByIdForContentAsync(
                request.ParentCommentId.Value,
                educationalContentId);

            if (parentComment is null)
            {
                throw new KeyNotFoundException(
                    "Parent comment not found for this educational content.");
            }
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EducationalContentId = educationalContentId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repository.Add(comment);
        await _repository.SaveChangesAsync();

        var contentOwnerId = await _repository
            .GetEducationalContentOwnerIdAsync(educationalContentId);

        if (contentOwnerId is not null && contentOwnerId != user.Id)
        {
            await _notificationsService.CreateAsync(
                contentOwnerId.Value,
                NotificationType.CommentReceived,
                "Someone commented on your educational content.",
                comment.Id);
        }

        return MapToResponse(comment);
    }

    public async Task<CommentResponse> UpdateAsync(
        string firebaseUid,
        Guid commentId,
        UpdateCommentRequest request)
    {
        // Validate the comment ID.
        if (commentId == Guid.Empty)
        {
            throw new ArgumentException("Comment ID is required.");
        }

        // Validate the updated comment content.
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Comment content is required.");
        }

        var user = await _repository.GetUserByFirebaseUidAsync(firebaseUid);

        // Make sure the Firebase user exists in the application database.
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var comment = await _repository.GetByIdAsync(commentId);

        // Make sure the comment exists.
        if (comment is null)
        {
            throw new KeyNotFoundException("Comment not found.");
        }

        // Only the comment owner can update it.
        if (comment.UserId != user.Id)
        {
            throw new InvalidOperationException(
                "You can only update your own comments.");
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return MapToResponse(comment);
    }

    public async Task DeleteAsync(
        string firebaseUid,
        Guid commentId)
    {
        // Validate the comment ID.
        if (commentId == Guid.Empty)
        {
            throw new ArgumentException("Comment ID is required.");
        }

        var user = await _repository.GetUserByFirebaseUidAsync(firebaseUid);

        // Make sure the Firebase user exists in the application database.
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var comment = await _repository.GetByIdAsync(commentId);

        // Make sure the comment exists.
        if (comment is null)
        {
            throw new KeyNotFoundException("Comment not found.");
        }

        // Only the comment owner can delete it.
        if (comment.UserId != user.Id)
        {
            throw new InvalidOperationException(
                "You can only delete your own comments.");
        }

        _repository.Remove(comment);
        await _repository.SaveChangesAsync();
    }

    private static CommentResponse MapToResponse(Comment comment)
    {
        // Map a comment entity to the API response DTO.
        return new CommentResponse
        {
            Id = comment.Id,
            UserId = comment.UserId,
            EducationalContentId = comment.EducationalContentId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}