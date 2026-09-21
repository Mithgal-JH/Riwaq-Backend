using System.Net;
using System.Text.Json;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.AI;

public sealed class ContentAnalysisService : IContentAnalysisService
{
    private const int MaximumTextLength = 20_000;
    private const string Language = "en";
    private readonly IContentAnalysisClient _client;
    private readonly IContentAnalysisRepository _repository;
    private readonly ILogger<ContentAnalysisService> _logger;
    private readonly IPostUpsertedClient? _postUpsertedClient;

    public ContentAnalysisService(
        IContentAnalysisClient client,
        IContentAnalysisRepository repository,
        ILogger<ContentAnalysisService> logger,
        IPostUpsertedClient? postUpsertedClient = null)
    {
        _client = client;
        _repository = repository;
        _logger = logger;
        _postUpsertedClient = postUpsertedClient;
    }

    public async Task AnalyzeAsync(
        EducationalContentModel content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var text = BuildText(content);
            var analysis = await GetOrCreateAnalysisAsync(content);

            if (analysis.AnalysisState == "Completed")
            {
                return;
            }

            var response = await _client.AnalyzeAsync(new ContentAnalysisRequest
            {
                ContentId = content.Id.ToString(),
                ContentVersion = content.ContentVersion,
                Text = text,
                Language = Language,
                RequestId = analysis.RequestId
            }, cancellationToken);

            if (string.IsNullOrWhiteSpace(response.RequestId)
                || !string.Equals(
                    response.RequestId,
                    analysis.RequestId,
                    StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "Ignoring content analysis response with invalid request ID for content {ContentId}, version {Version}.",
                    content.Id,
                    content.ContentVersion);
                await RecordFailureAsync(content, "request_id_mismatch");
                return;
            }

            if (response.ContentVersion != content.ContentVersion
                || !string.Equals(
                    response.ContentId,
                    content.Id.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Ignoring stale content analysis response for content {ContentId}, version {Version}.",
                    content.Id,
                    response.ContentVersion);
                return;
            }

            ApplyResponse(analysis, response);
            var saved = await _repository.SaveIfCurrentVersionAsync(analysis);

            if (!saved)
            {
                _logger.LogWarning(
                    "Ignoring content analysis response because content {ContentId} moved past version {Version}.",
                    content.Id,
                    content.ContentVersion);
            }
            else
            {
                await TryUpsertPostAsync(content, analysis, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (
            !cancellationToken.IsCancellationRequested)
        {
            await RecordFailureAsync(content, "timeout");
        }
        catch (AiServiceException exception)
        {
            await RecordFailureAsync(
                content,
                exception.StatusCode is HttpStatusCode statusCode
                    ? $"http_{(int)statusCode}"
                    : "ai_service_failure");
        }
        catch (HttpRequestException)
        {
            await RecordFailureAsync(content, "network_failure");
        }
        catch (ArgumentException)
        {
            await RecordFailureAsync(content, "invalid_content_text");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Content analysis failed unexpectedly for content {ContentId}, version {Version}.",
                content.Id,
                content.ContentVersion);
        }
    }

    private async Task<ContentAnalysis> GetOrCreateAnalysisAsync(
        EducationalContentModel content)
    {
        var existing = await _repository.GetByContentVersionAsync(
            content.Id,
            content.ContentVersion);

        if (existing is not null)
        {
            return existing;
        }

        var analysis = new ContentAnalysis
        {
            Id = Guid.NewGuid(),
            EducationalContentId = content.Id,
            ContentVersion = content.ContentVersion,
            RequestId = $"req_{Guid.NewGuid():N}",
            AnalysisState = "Pending"
        };

        _repository.Add(analysis);
        await _repository.SaveIfCurrentVersionAsync(analysis);
        return analysis;
    }

    private async Task RecordFailureAsync(
        EducationalContentModel content,
        string failureCode)
    {
        try
        {
            var analysis = await GetOrCreateAnalysisAsync(content);
            analysis.AnalysisState = "Failed";
            analysis.FailureCode = failureCode;
            await _repository.SaveIfCurrentVersionAsync(analysis);

            _logger.LogWarning(
                "Content analysis failed for content {ContentId}, version {Version}: {FailureCode}.",
                content.Id,
                content.ContentVersion,
                failureCode);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Could not persist content analysis failure for content {ContentId}, version {Version}.",
                content.Id,
                content.ContentVersion);
        }
    }

    private static string BuildText(EducationalContentModel content)
    {
        var parts = new[] { content.Title, content.Description }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToArray();
        var text = string.Join("\n\n", parts);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Content text is required.");
        }

        if (text.Length > MaximumTextLength)
        {
            throw new ArgumentException(
                "Content text cannot exceed 20,000 characters.");
        }

        return text;
    }

    private static void ApplyResponse(
        ContentAnalysis analysis,
        ContentAnalysisResponse response)
    {
        analysis.RequestId = response.RequestId;
        analysis.AnalysisState = "Completed";
        analysis.ProcessingStatus = response.ProcessingStatus;
        analysis.ProcessedAt = response.ProcessedAt;
        analysis.ClassificationStatus = response.Topics.ClassificationStatus;
        analysis.TopicReasonCode = response.Topics.ReasonCode;
        analysis.PrimaryTopicsJson = JsonSerializer.Serialize(
            response.Topics.PrimaryTopics);
        analysis.SecondaryTopicsJson = JsonSerializer.Serialize(
            response.Topics.SecondaryTopics);
        analysis.TopicCount = response.Topics.TopicCount;
        analysis.DifficultyLevel = response.Difficulty.Level;
        analysis.DifficultyConfidence = response.Difficulty.Confidence;
        analysis.SafetyStatus = response.Safety.Status;
        analysis.SafetyConfidence = response.Safety.Confidence;
        analysis.SafetyThreshold = response.Safety.Threshold;
        analysis.SafetyReviewRequired = response.Safety.ReviewRequired;
        analysis.RiskCategoriesJson = JsonSerializer.Serialize(
            response.Safety.RiskCategories);
        analysis.RecommendationSignal = response.Safety.RecommendationSignal;
        analysis.NeedsReview = response.NeedsReview;
        analysis.TopicModel = response.ModelVersions.TopicModel;
        analysis.DifficultyModel = response.ModelVersions.DifficultyModel;
        analysis.SafetyModel = response.ModelVersions.SafetyModel;
        analysis.PreprocessingVersion = response.PreprocessingVersion;
        analysis.FailureCode = null;
    }

    private async Task TryUpsertPostAsync(
        EducationalContentModel content,
        ContentAnalysis analysis,
        CancellationToken cancellationToken)
    {
        if (_postUpsertedClient is null)
        {
            return;
        }

        try
        {
            var primaryTopics = JsonSerializer.Deserialize<List<TopicResult>>(
                    analysis.PrimaryTopicsJson)
                ?? [];

            await _postUpsertedClient.UpsertAsync(new PostUpsertedRequest
            {
                PostId = content.Id.ToString(),
                CreatorId = content.UserId.ToString(),
                Title = content.Title,
                Body = BuildText(content),
                CreatedAt = new DateTimeOffset(content.CreatedAt),
                PrimaryTopic = primaryTopics.FirstOrDefault()?.Topic ?? string.Empty,
                Topics = new PostTopics { PrimaryTopics = primaryTopics },
                Difficulty = new DifficultyResult
                {
                    Level = analysis.DifficultyLevel ?? string.Empty,
                    Confidence = analysis.DifficultyConfidence ?? 0
                },
                Safety = new PostSafety
                {
                    Status = analysis.SafetyStatus ?? string.Empty,
                    RecommendationSignal = analysis.RecommendationSignal ?? string.Empty
                }
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Post indexing failed after content analysis for content {ContentId}, version {Version}.",
                content.Id,
                content.ContentVersion);
        }
    }
}
