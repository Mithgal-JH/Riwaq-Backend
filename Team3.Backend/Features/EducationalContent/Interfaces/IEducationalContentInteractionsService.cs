namespace Team3.Backend.Features.EducationalContent.Interfaces;

public interface IEducationalContentInteractionsService
{
    // Add a like to educational content.
    Task LikeAsync(
        string firebaseUid,
        Guid educationalContentId);

    // Remove the user's like from educational content.
    Task UnlikeAsync(
        string firebaseUid,
        Guid educationalContentId);

    // Add a save to educational content.
    Task SaveAsync(
        string firebaseUid,
        Guid educationalContentId);

    // Remove the user's save from educational content.
    Task UnsaveAsync(
        string firebaseUid,
        Guid educationalContentId);

    // Add a repost to educational content.
    Task RepostAsync(
        string firebaseUid,
        Guid educationalContentId);

    // Remove the user's repost from educational content.
    Task UnrepostAsync(
        string firebaseUid,
        Guid educationalContentId);

    // Record a share event for educational content.
    Task ShareAsync(
        string firebaseUid,
        Guid educationalContentId);
}