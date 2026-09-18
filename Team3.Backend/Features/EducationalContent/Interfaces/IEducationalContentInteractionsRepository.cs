using Team3.Backend.Models;

namespace Team3.Backend.Features.EducationalContent.Interfaces;

public interface IEducationalContentInteractionsRepository
{
    // Get the user's like for specific educational content.
    Task<Like?> GetLikeAsync(
        Guid userId,
        Guid educationalContentId);

    // Add a new like.
    void AddLike(Like like);

    // Remove an existing like.
    void RemoveLike(Like like);

    // Get the user's save for specific educational content.
    Task<Save?> GetSaveAsync(
        Guid userId,
        Guid educationalContentId);

    // Add a new save.
    void AddSave(Save save);

    // Remove an existing save.
    void RemoveSave(Save save);

    // Get the user's repost for specific educational content.
    Task<Repost?> GetRepostAsync(
        Guid userId,
        Guid educationalContentId);

    // Add a new repost.
    void AddRepost(Repost repost);

    // Remove an existing repost.
    void RemoveRepost(Repost repost);
    // Add a new share event.
    void AddShare(Share share);
    // Save changes to the database.
    Task SaveChangesAsync();
}