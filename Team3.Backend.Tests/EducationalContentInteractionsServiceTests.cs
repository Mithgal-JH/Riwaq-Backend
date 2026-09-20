using FluentAssertions;
using Moq;
using Team3.Backend.Features.EducationalContent;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class EducationalContentInteractionsServiceTests
{
    private readonly Mock<IEducationalContentRepository>
        _educationalContentRepositoryMock;

    private readonly Mock<IEducationalContentInteractionsRepository>
        _interactionsRepositoryMock;

    private readonly EducationalContentInteractionsService _service;

    public EducationalContentInteractionsServiceTests()
    {
        _educationalContentRepositoryMock = new Mock<
            IEducationalContentRepository>();

        _interactionsRepositoryMock = new Mock<
            IEducationalContentInteractionsRepository>();

        // Create the service with mocked repositories.
        _service = new EducationalContentInteractionsService(
            _educationalContentRepositoryMock.Object,
            _interactionsRepositoryMock.Object);
    }

    [Fact]
    public async Task LikeAsync_ShouldAddLike_WhenContentExistsAndUserHasNotLiked()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _interactionsRepositoryMock
            .Setup(x => x.GetLikeAsync(userId, contentId))
            .ReturnsAsync((Like?)null);

        // Act
        await _service.LikeAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.AddLike(It.Is<Like>(like =>
                like.UserId == userId &&
                like.EducationalContentId == contentId)),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task LikeAsync_ShouldThrowArgumentException_WhenContentIdIsEmpty()
    {
        // Arrange
        var firebaseUid = "firebase-uid";

        // Act
        var act = () => _service.LikeAsync(
            firebaseUid,
            Guid.Empty);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Educational content ID must be a valid Guid.");
    }

    [Fact]
    public async Task LikeAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var contentId = Guid.NewGuid();

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _service.LikeAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task LikeAsync_ShouldThrowKeyNotFoundException_WhenContentDoesNotExist()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(
                (Team3.Backend.Models.EducationalContent?)null);

        // Act
        var act = () => _service.LikeAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Educational content not found.");
    }

    [Fact]
    public async Task LikeAsync_ShouldThrowInvalidOperationException_WhenAlreadyLiked()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        var existingLike = new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EducationalContentId = contentId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _interactionsRepositoryMock
            .Setup(x => x.GetLikeAsync(userId, contentId))
            .ReturnsAsync(existingLike);

        // Act
        var act = () => _service.LikeAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Educational content is already liked.");
    }

    [Fact]
    public async Task UnlikeAsync_ShouldRemoveLike_WhenLikeExists()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var existingLike = new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EducationalContentId = contentId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _interactionsRepositoryMock
            .Setup(x => x.GetLikeAsync(userId, contentId))
            .ReturnsAsync(existingLike);

        // Act
        await _service.UnlikeAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.RemoveLike(existingLike),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UnlikeAsync_ShouldThrowKeyNotFoundException_WhenLikeDoesNotExist()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _interactionsRepositoryMock
            .Setup(x => x.GetLikeAsync(userId, contentId))
            .ReturnsAsync((Like?)null);

        // Act
        var act = () => _service.UnlikeAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Like not found.");
    }

    [Fact]
    public async Task SaveAsync_ShouldAddSave_WhenContentExistsAndNotSaved()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _interactionsRepositoryMock
            .Setup(x => x.GetSaveAsync(userId, contentId))
            .ReturnsAsync((Save?)null);

        // Act
        await _service.SaveAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.AddSave(It.Is<Save>(save =>
                save.UserId == userId &&
                save.EducationalContentId == contentId)),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task SaveAsync_ShouldThrowInvalidOperationException_WhenAlreadySaved()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        var existingSave = new Save
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EducationalContentId = contentId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _interactionsRepositoryMock
            .Setup(x => x.GetSaveAsync(userId, contentId))
            .ReturnsAsync(existingSave);

        // Act
        var act = () => _service.SaveAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Educational content is already saved.");
    }

    [Fact]
    public async Task UnsaveAsync_ShouldRemoveSave_WhenSaveExists()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var existingSave = new Save
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EducationalContentId = contentId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _interactionsRepositoryMock
            .Setup(x => x.GetSaveAsync(userId, contentId))
            .ReturnsAsync(existingSave);

        // Act
        await _service.UnsaveAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.RemoveSave(existingSave),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UnsaveAsync_ShouldThrowKeyNotFoundException_WhenSaveDoesNotExist()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _interactionsRepositoryMock
            .Setup(x => x.GetSaveAsync(userId, contentId))
            .ReturnsAsync((Save?)null);

        // Act
        var act = () => _service.UnsaveAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Save not found.");
    }

    [Fact]
    public async Task RepostAsync_ShouldAddRepost_WhenContentExistsAndNotReposted()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _interactionsRepositoryMock
            .Setup(x => x.GetRepostAsync(userId, contentId))
            .ReturnsAsync((Repost?)null);

        // Act
        await _service.RepostAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.AddRepost(It.Is<Repost>(repost =>
                repost.UserId == userId &&
                repost.EducationalContentId == contentId)),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task RepostAsync_ShouldThrowInvalidOperationException_WhenAlreadyReposted()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        var existingRepost = new Repost
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EducationalContentId = contentId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _interactionsRepositoryMock
            .Setup(x => x.GetRepostAsync(userId, contentId))
            .ReturnsAsync(existingRepost);

        // Act
        var act = () => _service.RepostAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Educational content is already reposted.");
    }

    [Fact]
    public async Task UnrepostAsync_ShouldRemoveRepost_WhenRepostExists()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var existingRepost = new Repost
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EducationalContentId = contentId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _interactionsRepositoryMock
            .Setup(x => x.GetRepostAsync(userId, contentId))
            .ReturnsAsync(existingRepost);

        // Act
        await _service.UnrepostAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.RemoveRepost(existingRepost),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UnrepostAsync_ShouldThrowKeyNotFoundException_WhenRepostDoesNotExist()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _interactionsRepositoryMock
            .Setup(x => x.GetRepostAsync(userId, contentId))
            .ReturnsAsync((Repost?)null);

        // Act
        var act = () => _service.UnrepostAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Repost not found.");
    }

    [Fact]
    public async Task ShareAsync_ShouldAddShare_WhenContentExists()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        var content = new Team3.Backend.Models.EducationalContent
        {
            Id = contentId,
            UserId = Guid.NewGuid()
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        // Act
        await _service.ShareAsync(firebaseUid, contentId);

        // Assert
        _interactionsRepositoryMock.Verify(
            x => x.AddShare(It.Is<Share>(share =>
                share.UserId == userId &&
                share.EducationalContentId == contentId)),
            Times.Once);

        _interactionsRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ShareAsync_ShouldThrowKeyNotFoundException_WhenContentDoesNotExist()
    {
        // Arrange
        var firebaseUid = "firebase-uid";
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var user = new User
        {
            Id = userId
        };

        _educationalContentRepositoryMock
            .Setup(x => x.GetUserByFirebaseUidAsync(firebaseUid))
            .ReturnsAsync(user);

        _educationalContentRepositoryMock
            .Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(
                (Team3.Backend.Models.EducationalContent?)null);

        // Act
        var act = () => _service.ShareAsync(
            firebaseUid,
            contentId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Educational content not found.");
    }
}