using System.Net;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.Recommendations.Dtos;
using Team3.Backend.Features.Recommendations.Interfaces;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;
using BackendPostRecommendationResponse = Team3.Backend.Features.Recommendations.Dtos.PostRecommendationResponse;
using BackendPostRecommendationItem = Team3.Backend.Features.Recommendations.Dtos.PostRecommendationItem;

namespace Team3.Backend.Features.Recommendations;

public sealed class PostRecommendationService : IPostRecommendationService
{
    private const int MaximumCandidates = 100;
    private readonly IUsersRepository _usersRepository;
    private readonly IPostRecommendationRepository _repository;
    private readonly IPostRecommendationClient _client;
    private readonly ILogger<PostRecommendationService> _logger;

    public PostRecommendationService(
        IUsersRepository usersRepository,
        IPostRecommendationRepository repository,
        IPostRecommendationClient client,
        ILogger<PostRecommendationService> logger)
    {
        _usersRepository = usersRepository;
        _repository = repository;
        _client = client;
        _logger = logger;
    }

    public async Task<BackendPostRecommendationResponse> RecommendAsync(
        Guid userId,
        int limit = 10,
        string? requestId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateLimit(limit);

        var user = await _usersRepository.GetByIdForAiSyncAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var learningDirection = user.SelectedSkill?.Name;

        if (string.IsNullOrWhiteSpace(learningDirection))
        {
            throw new ArgumentException(
                "A learning direction is required for post recommendations.");
        }

        var candidateIds = await _repository
            .GetEligibleCandidateIdsAsync(MaximumCandidates);
        candidateIds = candidateIds
            .Take(MaximumCandidates)
            .ToList();
        var interactions = await _repository.GetRecentInteractionsAsync(userId);
        var effectiveRequestId = string.IsNullOrWhiteSpace(requestId)
            ? $"req_{Guid.NewGuid():N}"
            : requestId;
        var request = new PostRecommendationRequest
        {
            RequestId = effectiveRequestId,
            UserId = userId.ToString(),
            DeclaredTopics = user.UserSkills
                .Select(userSkill => userSkill.Skill.Name)
                .Concat(user.UserInterests.Select(userInterest => userInterest.Interest.Name))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            LearningDirection = learningDirection,
            EligibleCandidateIds = candidateIds
                .Select(id => id.ToString())
                .ToList(),
            ExcludePostIds = null,
            Limit = limit,
            RecentInteractions = interactions.Count == 0
                ? null
                : interactions
        };

        try
        {
            var response = await _client.RecommendPostsAsync(
                request,
                cancellationToken);

            return await HydrateAsync(response, cancellationToken);
        }
        catch (AiServiceException exception) when (
            exception.StatusCode is null
                or HttpStatusCode.InternalServerError)
        {
            _logger.LogWarning(
                exception,
                "Post recommendation AI call failed; returning chronological fallback for user {UserId}.",
                userId);
            return await BuildFallbackAsync(
                effectiveRequestId,
                candidateIds,
                limit,
                cancellationToken);
        }
    }

    private async Task<BackendPostRecommendationResponse> HydrateAsync(
        Team3.Backend.Features.AI.Dtos.PostRecommendationResponse response,
        CancellationToken cancellationToken)
    {
        var ids = response.Items
            .Select(item => Guid.TryParse(item.Id, out var id)
                ? id
                : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var posts = await _repository.GetByIdsAsync(ids);
        var postsById = posts.ToDictionary(post => post.Id);
        var items = new List<BackendPostRecommendationItem>();

        foreach (var item in response.Items)
        {
            if (!Guid.TryParse(item.Id, out var postId)
                || !postsById.TryGetValue(postId, out var post))
            {
                continue;
            }

            items.Add(new BackendPostRecommendationItem
            {
                Post = MapPost(post),
                Rank = item.Rank,
                Score = item.Score,
                ReasonCodes = item.ReasonCodes,
                PrimaryTopic = item.PrimaryTopic
            });
        }

        return new BackendPostRecommendationResponse
        {
            RequestId = response.RequestId,
            ModelVersion = response.ModelVersion,
            Count = items.Count,
            Items = items
        };
    }

    private async Task<BackendPostRecommendationResponse> BuildFallbackAsync(
        string requestId,
        IReadOnlyList<Guid> candidateIds,
        int limit,
        CancellationToken cancellationToken)
    {
        var posts = await _repository.GetByIdsAsync(candidateIds);
        var orderedPosts = posts
            .OrderByDescending(post => post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Take(limit)
            .ToList();

        return new BackendPostRecommendationResponse
        {
            RequestId = requestId,
            Count = orderedPosts.Count,
            Items = orderedPosts.Select((post, index) => new BackendPostRecommendationItem
            {
                Post = MapPost(post),
                Rank = index + 1
            }).ToList()
        };
    }

    private static EducationalContentResponse MapPost(
        EducationalContentModel post)
    {
        return new EducationalContentResponse
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title,
            Description = post.Description,
            ContentType = post.ContentType,
            ContentUrl = post.ContentUrl,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID must be a valid Guid.");
        }
    }

    private static void ValidateLimit(int limit)
    {
        if (limit is < 1 or > 50)
        {
            throw new ArgumentException("limit must be between 1 and 50.");
        }
    }
}
