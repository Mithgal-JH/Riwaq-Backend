using Team3.Backend.Features.LearningSessions.Dtos;
using Team3.Backend.Features.LearningSessions.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.LearningSessions;

public sealed class LearningSessionsService : ILearningSessionsService
{
    private static readonly string[] AllowedStatuses =
    [
        "Scheduled",
        "Completed",
        "Cancelled"
    ];

    private readonly ILearningSessionsRepository _repository;

    public LearningSessionsService(ILearningSessionsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<LearningSessionResponse>> GetMySessionsAsync(
        Guid userId)
    {
        ValidateUserId(userId);
        var sessions = await _repository.GetByUserIdAsync(userId);

        return sessions.Select(MapToResponse).ToList();
    }

    public async Task<LearningSessionResponse?> GetByIdAsync(
        Guid userId,
        Guid sessionId)
    {
        ValidateUserId(userId);
        ValidateId(sessionId);

        var session = await _repository.GetByIdForUserAsync(sessionId, userId);

        return session is null ? null : MapToResponse(session);
    }

    public async Task<LearningSessionResponse> CreateAsync(
        Guid userId,
        CreateLearningSessionRequest request)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(request.ConnectionId, "Connection ID");
        ValidateTitle(request.Title);

        if (request.ScheduledAt == default)
        {
            throw new ArgumentException("Scheduled date and time is required.");
        }

        var connection = await _repository.GetConnectionAsync(request.ConnectionId)
            ?? throw new KeyNotFoundException("Connection not found.");

        if (!ContainsUser(connection, userId))
        {
            throw new InvalidOperationException(
                "Only connection participants can create learning sessions.");
        }

        var now = DateTime.UtcNow;
        var session = new LearningSession
        {
            Id = Guid.NewGuid(),
            ConnectionId = connection.Id,
            Title = request.Title.Trim(),
            Description = request.Description,
            ScheduledAt = request.ScheduledAt,
            MeetingUrl = request.MeetingUrl,
            Status = "Scheduled",
            CreatedAt = now,
            UpdatedAt = now
        };

        _repository.Add(session);
        await _repository.SaveChangesAsync();

        return MapToResponse(session);
    }

    public async Task<LearningSessionResponse> UpdateAsync(
        Guid userId,
        Guid sessionId,
        UpdateLearningSessionRequest request)
    {
        ValidateUserId(userId);
        ValidateId(sessionId);
        ArgumentNullException.ThrowIfNull(request);

        var session = await _repository.GetTrackedByIdForUserAsync(
            sessionId,
            userId);

        if (session is null)
        {
            throw new KeyNotFoundException("Learning session not found.");
        }

        if (request.Title is not null)
        {
            ValidateTitle(request.Title);
            session.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            session.Description = request.Description;
        }

        if (request.ScheduledAt.HasValue)
        {
            if (request.ScheduledAt.Value == default)
            {
                throw new ArgumentException(
                    "Scheduled date and time must be valid.");
            }

            session.ScheduledAt = request.ScheduledAt.Value;
        }

        if (request.MeetingUrl is not null)
        {
            session.MeetingUrl = request.MeetingUrl;
        }

        if (request.Status is not null)
        {
            session.Status = NormalizeStatus(request.Status);
        }

        session.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        return MapToResponse(session);
    }

    private static bool ContainsUser(Connection connection, Guid userId)
    {
        return connection.UserAId == userId || connection.UserBId == userId;
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status is required.");
        }

        var normalized = AllowedStatuses.FirstOrDefault(candidate =>
            candidate.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

        return normalized
            ?? throw new ArgumentException(
                "Status must be one of: Scheduled, Completed, Cancelled.");
    }

    private static void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }
    }

    private static void ValidateId(Guid id, string name = "ID")
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{name} must be a valid Guid.");
        }
    }

    private static void ValidateUserId(Guid userId)
    {
        ValidateId(userId, "User ID");
    }

    private static LearningSessionResponse MapToResponse(
        LearningSession session) => new()
        {
            Id = session.Id,
            ConnectionId = session.ConnectionId,
            Title = session.Title,
            Description = session.Description,
            ScheduledAt = session.ScheduledAt,
            MeetingUrl = session.MeetingUrl,
            Status = session.Status,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
}