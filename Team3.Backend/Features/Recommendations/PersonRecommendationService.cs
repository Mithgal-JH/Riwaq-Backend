using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Recommendations.Dtos;
using Team3.Backend.Features.Recommendations.Interfaces;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Recommendations;

public sealed class PersonRecommendationService : IPersonRecommendationService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPersonRecommendationClient _personRecommendationClient;

    public PersonRecommendationService(
        IUsersRepository usersRepository,
        IPersonRecommendationClient personRecommendationClient)
    {
        _usersRepository = usersRepository;
        _personRecommendationClient = personRecommendationClient;
    }

    public async Task<PersonRecommendationResponse> GetForUserAsync(
        Guid userId,
        int topN = 5,
        string? requestId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateTopN(topN);

        var requestingUser = await _usersRepository.GetByIdWithProfileAsync(userId);

        if (requestingUser is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var effectiveRequestId = string.IsNullOrWhiteSpace(requestId)
            ? $"req_{Guid.NewGuid():N}"
            : requestId;

        var aiResponse = await _personRecommendationClient.GetRecommendationsAsync(
            userId.ToString(),
            topN,
            effectiveRequestId,
            cancellationToken);

        var candidateIds = aiResponse.Recommendations
            .Select(recommendation => recommendation.CandidateProfileId)
            .Select(id => Guid.TryParse(id, out var parsedId)
                ? parsedId
                : (Guid?)null)
            .Where(id => id.HasValue && id.Value != userId)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var candidates = candidateIds.Count == 0
            ? []
            : await _usersRepository.GetPublicProfilesByIdsAsync(candidateIds);

        var candidatesById = candidates.ToDictionary(user => user.Id);
        var recommendations = new List<PersonRecommendationItem>();
        var rank = 1;

        foreach (var recommendation in aiResponse.Recommendations)
        {
            if (!Guid.TryParse(
                    recommendation.CandidateProfileId,
                    out var candidateId)
                || candidateId == userId
                || !candidatesById.TryGetValue(candidateId, out var candidate))
            {
                continue;
            }

            recommendations.Add(new PersonRecommendationItem
            {
                Rank = rank++,
                Profile = MapProfile(candidate),
                SimilarityScore = recommendation.SimilarityScore,
                SharedSkills = recommendation.SharedSkills,
                SharedInterests = recommendation.SharedInterests,
                SameLearningDirection = recommendation.SameLearningDirection
            });
        }

        return new PersonRecommendationResponse
        {
            RequestId = aiResponse.RequestId,
            ProfileId = aiResponse.ProfileId,
            ProcessingStatus = aiResponse.ProcessingStatus,
            ProcessedAt = aiResponse.ProcessedAt,
            Recommendations = recommendations,
            RecommendationCount = recommendations.Count,
            LowConfidence = aiResponse.LowConfidence
        };
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID must be a valid Guid.");
        }
    }

    private static void ValidateTopN(int topN)
    {
        if (topN is < 1 or > 20)
        {
            throw new ArgumentException("topN must be between 1 and 20.");
        }
    }

    private static PublicUserProfileResponse MapProfile(User user)
    {
        return new PublicUserProfileResponse
        {
            UserId = user.Id,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            LearningDirectionName = user.SelectedSkill?.Name,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }
}
