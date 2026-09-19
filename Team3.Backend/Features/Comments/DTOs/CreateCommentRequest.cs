namespace Team3.Backend.Features.Comments.DTOs;

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }
}