namespace Team3.Backend.Features.Comments.DTOs;

public class CommentResponse
{
    // Comment identifier.
    public Guid Id { get; set; }

    // User who created the comment.
    public Guid UserId { get; set; }

    // Educational content this comment belongs to.
    public Guid EducationalContentId { get; set; }

    // Parent comment ID when this comment is a reply.
    public Guid? ParentCommentId { get; set; }

    // Comment text.
    public string Content { get; set; } = string.Empty;

    // Comment creation and update timestamps.
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}