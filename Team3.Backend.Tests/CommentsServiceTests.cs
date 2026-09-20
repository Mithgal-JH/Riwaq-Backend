
using FluentAssertions;
using Moq;
using Team3.Backend.Features.Comments;
using Team3.Backend.Features.Comments.DTOs;
using Team3.Backend.Features.Comments.Interfaces;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class CommentsServiceTests
{
    private readonly Mock<ICommentsRepository> _repository = new();
    private readonly Mock<INotificationsService> _notificationsService = new();
    private readonly CommentsService _service;

    public CommentsServiceTests()
    {
        _service = new CommentsService(
            _repository.Object,
            _notificationsService.Object);
    }

    // Get comments when the educational content exists.
    [Fact]
    public async Task GetByContentIdAsync_ShouldReturnComments_WhenContentExists()
    {
        var contentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comments = new List<Comment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EducationalContentId = contentId,
                Content = "First comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EducationalContentId = contentId,
                Content = "Second comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        _repository
            .Setup(x => x.GetByContentIdAsync(contentId))
            .ReturnsAsync(comments);

        var response = await _service.GetByContentIdAsync(contentId);

        response.Should().HaveCount(2);
        response[0].Content.Should().Be("First comment");
        response[1].Content.Should().Be("Second comment");
    }

    // Reject the request when the educational content ID is empty.
    [Fact]
    public async Task GetByContentIdAsync_ShouldThrow_WhenContentIdIsEmpty()
    {
        var action = () => _service.GetByContentIdAsync(Guid.Empty);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Educational content ID is required.");
    }

    // Reject the request when the educational content does not exist.
    [Fact]
    public async Task GetByContentIdAsync_ShouldThrow_WhenContentDoesNotExist()
    {
        var contentId = Guid.NewGuid();

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(false);

        var action = () => _service.GetByContentIdAsync(contentId);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Educational content not found.");
    }

    // Create a comment when all required data is valid.
    [Fact]
    public async Task CreateAsync_ShouldCreateComment_WhenRequestIsValid()
    {
        var contentId = Guid.NewGuid();
        var user = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetEducationalContentOwnerIdAsync(contentId))
            .ReturnsAsync(Guid.NewGuid());

        var request = new CreateCommentRequest
        {
            Content = "  This is a comment.  "
        };

        var response = await _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        response.Content.Should().Be("This is a comment.");
        response.UserId.Should().Be(user.Id);
        response.EducationalContentId.Should().Be(contentId);

        _repository.Verify(
            x => x.Add(It.Is<Comment>(comment =>
                comment.UserId == user.Id &&
                comment.EducationalContentId == contentId &&
                comment.Content == "This is a comment.")),
            Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    // Reject comment creation when the educational content ID is empty.
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenContentIdIsEmpty()
    {
        var request = new CreateCommentRequest
        {
            Content = "Test comment"
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            Guid.Empty,
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Educational content ID is required.");
    }

    // Reject comment creation when the educational content does not exist.
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenContentDoesNotExist()
    {
        var contentId = Guid.NewGuid();

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(false);

        var request = new CreateCommentRequest
        {
            Content = "Test comment"
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Educational content not found.");
    }

    // Reject comment creation when the comment text is empty.
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenContentIsEmpty()
    {
        var contentId = Guid.NewGuid();

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        var request = new CreateCommentRequest
        {
            Content = "   "
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Comment content is required.");
    }

    // Reject comment creation when the Firebase user is not found.
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var contentId = Guid.NewGuid();

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync((User?)null);

        var request = new CreateCommentRequest
        {
            Content = "Test comment"
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    // Reject a reply when the parent comment does not belong to the same content.
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenParentCommentDoesNotBelongToContent()
    {
        var contentId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var user = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdForContentAsync(
                parentCommentId,
                contentId))
            .ReturnsAsync((Comment?)null);

        var request = new CreateCommentRequest
        {
            Content = "Reply",
            ParentCommentId = parentCommentId
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage(
                "Parent comment not found for this educational content.");
    }

    // Notify the content owner when another user adds a comment.
    [Fact]
    public async Task CreateAsync_ShouldCreateNotification_WhenContentOwnerIsDifferent()
    {
        var contentId = Guid.NewGuid();
        var user = ServiceTestData.User(Guid.NewGuid());
        var ownerId = Guid.NewGuid();

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetEducationalContentOwnerIdAsync(contentId))
            .ReturnsAsync(ownerId);

        var request = new CreateCommentRequest
        {
            Content = "Test comment"
        };

        await _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        _notificationsService.Verify(
            x => x.CreateAsync(
                ownerId,
                NotificationType.CommentReceived,
                "Someone commented on your educational content.",
                It.IsAny<Guid>()),
            Times.Once);
    }

    // Do not notify the user when they comment on their own content.
    [Fact]
    public async Task CreateAsync_ShouldNotCreateNotification_WhenUserIsContentOwner()
    {
        var contentId = Guid.NewGuid();
        var user = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetEducationalContentOwnerIdAsync(contentId))
            .ReturnsAsync(user.Id);

        var request = new CreateCommentRequest
        {
            Content = "My own content comment"
        };

        await _service.CreateAsync(
            "firebase-uid",
            contentId,
            request);

        _notificationsService.Verify(
            x => x.CreateAsync(
                It.IsAny<Guid>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    // Update a comment when the authenticated user owns it.
    [Fact]
    public async Task UpdateAsync_ShouldUpdateComment_WhenUserOwnsComment()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var commentId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = user.Id,
            EducationalContentId = Guid.NewGuid(),
            Content = "Old content",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        var request = new UpdateCommentRequest
        {
            Content = "  Updated content  "
        };

        var response = await _service.UpdateAsync(
            "firebase-uid",
            commentId,
            request);

        response.Content.Should().Be("Updated content");
        comment.Content.Should().Be("Updated content");

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    // Reject the update when the comment ID is empty.
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCommentIdIsEmpty()
    {
        var request = new UpdateCommentRequest
        {
            Content = "Updated"
        };

        var action = () => _service.UpdateAsync(
            "firebase-uid",
            Guid.Empty,
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Comment ID is required.");
    }

    // Reject the update when the new comment text is empty.
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenContentIsEmpty()
    {
        var request = new UpdateCommentRequest
        {
            Content = "   "
        };

        var action = () => _service.UpdateAsync(
            "firebase-uid",
            Guid.NewGuid(),
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Comment content is required.");
    }

    // Reject the update when the Firebase user is not found.
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var commentId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync((User?)null);

        var request = new UpdateCommentRequest
        {
            Content = "Updated"
        };

        var action = () => _service.UpdateAsync(
            "firebase-uid",
            commentId,
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    // Reject the update when the comment does not exist.
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var commentId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        var request = new UpdateCommentRequest
        {
            Content = "Updated"
        };

        var action = () => _service.UpdateAsync(
            "firebase-uid",
            commentId,
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Comment not found.");
    }

    // Prevent a user from updating another user's comment.
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUserDoesNotOwnComment()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var commentId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = Guid.NewGuid(),
            EducationalContentId = Guid.NewGuid(),
            Content = "Original"
        };

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        var request = new UpdateCommentRequest
        {
            Content = "Updated"
        };

        var action = () => _service.UpdateAsync(
            "firebase-uid",
            commentId,
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You can only update your own comments.");
    }

    // Delete a comment when the authenticated user owns it.
    [Fact]
    public async Task DeleteAsync_ShouldDeleteComment_WhenUserOwnsComment()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var commentId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = user.Id,
            EducationalContentId = Guid.NewGuid(),
            Content = "Comment"
        };

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        await _service.DeleteAsync(
            "firebase-uid",
            commentId);

        _repository.Verify(
            x => x.Remove(comment),
            Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    // Reject deletion when the comment ID is empty.
    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCommentIdIsEmpty()
    {
        var action = () => _service.DeleteAsync(
            "firebase-uid",
            Guid.Empty);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Comment ID is required.");
    }

    // Reject deletion when the Firebase user is not found.
    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var commentId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync((User?)null);

        var action = () => _service.DeleteAsync(
            "firebase-uid",
            commentId);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    // Reject deletion when the comment does not exist.
    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var commentId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        var action = () => _service.DeleteAsync(
            "firebase-uid",
            commentId);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Comment not found.");
    }

    // Prevent a user from deleting another user's comment.
    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenUserDoesNotOwnComment()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var commentId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = Guid.NewGuid(),
            EducationalContentId = Guid.NewGuid(),
            Content = "Comment"
        };

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        var action = () => _service.DeleteAsync(
            "firebase-uid",
            commentId);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You can only delete your own comments.");
    }
}
