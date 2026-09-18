using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.EducationalContent;

public class EducationalContentInteractionsService
    : IEducationalContentInteractionsService
{
    private readonly IEducationalContentRepository _educationalContentRepository;
    private readonly IEducationalContentInteractionsRepository _interactionsRepository;

    public EducationalContentInteractionsService(
        IEducationalContentRepository educationalContentRepository,
        IEducationalContentInteractionsRepository interactionsRepository)
    {
        _educationalContentRepository = educationalContentRepository;
        _interactionsRepository = interactionsRepository;
    }

    // Add a like to educational content.
    public async Task LikeAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check that the educational content exists.
        var content = await _educationalContentRepository
            .GetByIdAsync(educationalContentId);

        if (content is null)
        {
            throw new KeyNotFoundException(
                "Educational content not found.");
        }

        // Check if the user already liked this content.
        var existingLike = await _interactionsRepository.GetLikeAsync(
            user.Id,
            educationalContentId);

        if (existingLike is not null)
        {
            throw new InvalidOperationException(
                "Educational content is already liked.");
        }

        // Create a new like.
        var like = new Like
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EducationalContentId = educationalContentId,
            CreatedAt = DateTime.UtcNow
        };

        _interactionsRepository.AddLike(like);

        await _interactionsRepository.SaveChangesAsync();
    }

    // Remove the user's like from educational content.
    public async Task UnlikeAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check if the user has liked this content.
        var existingLike = await _interactionsRepository.GetLikeAsync(
            user.Id,
            educationalContentId);

        if (existingLike is null)
        {
            throw new KeyNotFoundException("Like not found.");
        }

        // Remove the existing like.
        _interactionsRepository.RemoveLike(existingLike);

        await _interactionsRepository.SaveChangesAsync();
    }

    // Add a save to educational content.
    public async Task SaveAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check that the educational content exists.
        var content = await _educationalContentRepository
            .GetByIdAsync(educationalContentId);

        if (content is null)
        {
            throw new KeyNotFoundException(
                "Educational content not found.");
        }

        // Check if the user already saved this content.
        var existingSave = await _interactionsRepository.GetSaveAsync(
            user.Id,
            educationalContentId);

        if (existingSave is not null)
        {
            throw new InvalidOperationException(
                "Educational content is already saved.");
        }

        // Create a new save.
        var save = new Save
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EducationalContentId = educationalContentId,
            CreatedAt = DateTime.UtcNow
        };

        _interactionsRepository.AddSave(save);

        await _interactionsRepository.SaveChangesAsync();
    }

    // Remove the user's save from educational content.
    public async Task UnsaveAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check if the user has saved this content.
        var existingSave = await _interactionsRepository.GetSaveAsync(
            user.Id,
            educationalContentId);

        if (existingSave is null)
        {
            throw new KeyNotFoundException("Save not found.");
        }

        // Remove the existing save.
        _interactionsRepository.RemoveSave(existingSave);

        await _interactionsRepository.SaveChangesAsync();
    }

    // Add a repost to educational content.
    public async Task RepostAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check that the educational content exists.
        var content = await _educationalContentRepository
            .GetByIdAsync(educationalContentId);

        if (content is null)
        {
            throw new KeyNotFoundException(
                "Educational content not found.");
        }

        // Check if the user already reposted this content.
        var existingRepost = await _interactionsRepository.GetRepostAsync(
            user.Id,
            educationalContentId);

        if (existingRepost is not null)
        {
            throw new InvalidOperationException(
                "Educational content is already reposted.");
        }

        // Create a new repost.
        var repost = new Repost
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EducationalContentId = educationalContentId,
            CreatedAt = DateTime.UtcNow
        };

        _interactionsRepository.AddRepost(repost);

        await _interactionsRepository.SaveChangesAsync();
    }

    // Remove the user's repost from educational content.
    public async Task UnrepostAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check if the user has reposted this content.
        var existingRepost = await _interactionsRepository.GetRepostAsync(
            user.Id,
            educationalContentId);

        if (existingRepost is null)
        {
            throw new KeyNotFoundException("Repost not found.");
        }

        // Remove the existing repost.
        _interactionsRepository.RemoveRepost(existingRepost);

        await _interactionsRepository.SaveChangesAsync();
    }
        // Record a share event for educational content.
    public async Task ShareAsync(
        string firebaseUid,
        Guid educationalContentId)
    {
        // Validate the content ID.
        if (educationalContentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }

        // Get the current user from Firebase UID.
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Check that the educational content exists.
        var content = await _educationalContentRepository
            .GetByIdAsync(educationalContentId);

        if (content is null)
        {
            throw new KeyNotFoundException(
                "Educational content not found.");
        }

        // Create a new share event.
        var share = new Share
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EducationalContentId = educationalContentId,
            CreatedAt = DateTime.UtcNow
        };

        _interactionsRepository.AddShare(share);

        await _interactionsRepository.SaveChangesAsync();
    }
}