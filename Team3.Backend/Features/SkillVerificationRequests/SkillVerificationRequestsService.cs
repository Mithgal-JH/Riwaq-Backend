using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Features.SkillVerificationRequests.Dtos;
using Team3.Backend.Features.SkillVerificationRequests.Interfaces;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.SkillVerificationRequests;

public sealed class SkillVerificationRequestsService : ISkillVerificationRequestsService
{
    private readonly ISkillVerificationRequestsRepository _repository;
    private readonly INotificationsService? _notificationsService;

    public SkillVerificationRequestsService(
        ISkillVerificationRequestsRepository repository,
        INotificationsService? notificationsService = null)
    {
        _repository = repository;
        _notificationsService = notificationsService;
    }

    public async Task<SkillVerificationRequestResponse> CreateAsync(
        Guid userId,
        CreateSkillVerificationRequest request)
    {
        ValidateUserId(userId);

        if (request is null)
        {
            throw new ArgumentException("Request payload is required.");
        }

        if (request.MentorUserId == Guid.Empty)
        {
            throw new ArgumentException("MentorUserId must be a valid Guid.");
        }

        if (request.SkillId == Guid.Empty)
        {
            throw new ArgumentException("SkillId must be a valid Guid.");
        }

        if (userId == request.MentorUserId)
        {
            throw new InvalidOperationException(
                "You cannot request skill verification from yourself.");
        }

        var requester = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("Requester user not found.");

        var mentor = await _repository.GetUserByIdAsync(request.MentorUserId)
            ?? throw new KeyNotFoundException("Mentor user not found.");

        var skill = await _repository.GetSkillByIdAsync(request.SkillId)
            ?? throw new KeyNotFoundException("Skill not found.");

        var hasSharedSession = await _repository.HasSharedLearningSessionAsync(
            userId,
            request.MentorUserId);

        if (!hasSharedSession)
        {
            throw new InvalidOperationException(
                "Only users with at least one shared learning session can request skill verification.");
        }

        var existingPendingRequest = await _repository.GetPendingRequestForSkillAsync(
            userId,
            request.MentorUserId,
            request.SkillId);

        if (existingPendingRequest is not null)
        {
            throw new InvalidOperationException(
                "A pending skill verification request already exists for this mentor and skill.");
        }

        var model = new SkillVerificationRequest
        {
            Id = Guid.NewGuid(),
            RequesterUserId = userId,
            MentorUserId = request.MentorUserId,
            SkillId = request.SkillId,
            Status = "Pending",
            Score = null,
            Note = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RequesterUser = requester,
            MentorUser = mentor,
            Skill = skill
        };

        _repository.Add(model);
        await _repository.SaveChangesAsync();

        if (_notificationsService is not null)
        {
            await _notificationsService.CreateAsync(
                mentor.Id,
                NotificationType.SkillVerificationRequestReceived,
                $"You received a new skill verification request for {skill.Name}.",
                model.Id);
        }

        return MapToResponse(model);
    }

    public async Task<IReadOnlyList<SkillVerificationRequestResponse>> GetSentAsync(Guid userId)
    {
        ValidateUserId(userId);

        var user = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        _ = user;

        var requests = await _repository.GetSentAsync(userId);

        return requests
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<IReadOnlyList<SkillVerificationRequestResponse>> GetReceivedAsync(Guid userId)
    {
        ValidateUserId(userId);

        var user = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        _ = user;

        var requests = await _repository.GetReceivedAsync(userId);

        return requests
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<SkillVerificationRequestResponse> UpdateAsync(
        Guid userId,
        Guid requestId,
        UpdateSkillVerificationRequest request)
    {
        ValidateUserId(userId);

        if (request is null)
        {
            throw new ArgumentException("Request payload is required.");
        }

        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("Request ID must be a valid Guid.");
        }

        var currentUser = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var skillVerificationRequest = await _repository.GetRequestByIdAsync(requestId)
            ?? throw new KeyNotFoundException("Skill verification request not found.");

        var normalizedStatus = NormalizeStatus(request.Status);

        if (skillVerificationRequest.Status != "Pending")
        {
            throw new InvalidOperationException(
                "This skill verification request can no longer be changed.");
        }

        bool isRequester = skillVerificationRequest.RequesterUserId == currentUser.Id;
        bool isMentor = skillVerificationRequest.MentorUserId == currentUser.Id;

        if (!isRequester && !isMentor)
        {
            throw new InvalidOperationException(
                "You are not allowed to update this skill verification request.");
        }

        if (isRequester)
        {
            if (normalizedStatus != "Cancelled")
            {
                throw new InvalidOperationException(
                    "Only the requester can cancel a pending skill verification request.");
            }

            skillVerificationRequest.Status = "Cancelled";
            skillVerificationRequest.Score = null;
            skillVerificationRequest.Note = null;
        }
        else
        {
            if (normalizedStatus != "Accepted" && normalizedStatus != "Rejected")
            {
                throw new InvalidOperationException(
                    "The mentor can only accept or reject this request.");
            }

            skillVerificationRequest.Status = normalizedStatus;
            skillVerificationRequest.Score = normalizedStatus == "Accepted" ? 5 : null;

            var note = string.IsNullOrWhiteSpace(request.Note)
                ? null
                : request.Note.Trim();

            skillVerificationRequest.Note = note;
        }

        skillVerificationRequest.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        if (_notificationsService is not null)
        {
            if (isMentor && normalizedStatus == "Accepted")
            {
                await _notificationsService.CreateAsync(
                    skillVerificationRequest.RequesterUserId,
                    NotificationType.SkillVerificationRequestAccepted,
                    "Your skill verification request was accepted.",
                    skillVerificationRequest.Id);
            }
            else if (isMentor && normalizedStatus == "Rejected")
            {
                await _notificationsService.CreateAsync(
                    skillVerificationRequest.RequesterUserId,
                    NotificationType.SkillVerificationRequestRejected,
                    "Your skill verification request was rejected.",
                    skillVerificationRequest.Id);
            }
            else if (isRequester && normalizedStatus == "Cancelled")
            {
                await _notificationsService.CreateAsync(
                    skillVerificationRequest.MentorUserId,
                    NotificationType.SkillVerificationRequestCancelled,
                    "A skill verification request was cancelled.",
                    skillVerificationRequest.Id);
            }
        }

        return MapToResponse(skillVerificationRequest);
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID must be a valid Guid.");
        }
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status is required.");
        }

        var normalized = status.Trim();

        return normalized.ToLowerInvariant() switch
        {
            "pending" => "Pending",
            "accepted" => "Accepted",
            "rejected" => "Rejected",
            "cancelled" => "Cancelled",
            _ => throw new ArgumentException(
                "Status must be one of: Pending, Accepted, Rejected, Cancelled.")
        };
    }

    private static SkillVerificationRequestResponse MapToResponse(
        SkillVerificationRequest request)
    {
        return new SkillVerificationRequestResponse
        {
            Id = request.Id,
            Requester = MapProfile(request.RequesterUser),
            Mentor = MapProfile(request.MentorUser),
            Skill = new SkillResponse
            {
                Id = request.Skill.Id,
                Name = request.Skill.Name,
                Description = request.Skill.Description
            },
            Status = request.Status,
            Score = request.Score,
            Note = request.Note,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
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
