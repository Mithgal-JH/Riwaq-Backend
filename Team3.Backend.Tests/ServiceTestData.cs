using Team3.Backend.Models;

namespace Team3.Backend.Tests;

internal static class ServiceTestData
{
    public static User User(Guid id) => new()
    {
        Id = id,
        FirebaseUid = $"firebase-{id}",
        Email = $"{id}@example.com",
        UserName = $"{id}@example.com",
        SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    public static Experience Experience(Guid id, Guid userId) => new()
    {
        Id = id,
        UserId = userId,
        Title = "Existing experience",
        Description = "Existing description"
    };

    public static LearningDirection LearningDirection(Guid id) => new()
    {
        Id = id,
        Name = "Backend Development",
        Description = "Build APIs"
    };

    public static Team3.Backend.Models.Progress Progress(
        Guid id,
        Guid userId,
        Guid learningDirectionId) => new()
        {
            Id = id,
            UserId = userId,
            LearningDirectionId = learningDirectionId,
            Level = "Beginner",
            StartedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            LearningDirection = LearningDirection(learningDirectionId)
        };
}
