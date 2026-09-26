using FluentAssertions;
using Moq;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Features.SkillVerificationRequests;
using Team3.Backend.Features.SkillVerificationRequests.Dtos;
using Team3.Backend.Features.SkillVerificationRequests.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class SkillVerificationRequestsServiceTests
{
    private readonly Mock<ISkillVerificationRequestsRepository> _repository = new();
    private readonly Mock<INotificationsService> _notificationsService = new();
    private readonly SkillVerificationRequestsService _service;

    private readonly Guid _requesterId = Guid.NewGuid();
    private readonly Guid _mentorId = Guid.NewGuid();
    private readonly Guid _skillId = Guid.NewGuid();

    public SkillVerificationRequestsServiceTests()
    {
        _service = new SkillVerificationRequestsService(
            _repository.Object,
            _notificationsService.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectUsersWithoutSharedLearningSession()
    {
        var requester = ServiceTestData.User(_requesterId);
        var mentor = ServiceTestData.User(_mentorId);

        _repository.Setup(x => x.GetUserByIdAsync(_requesterId))
            .ReturnsAsync(requester);
        _repository.Setup(x => x.GetUserByIdAsync(_mentorId))
            .ReturnsAsync(mentor);
        _repository.Setup(x => x.GetSkillByIdAsync(_skillId))
            .ReturnsAsync(new Skill { Id = _skillId, Name = "C#" });
        _repository.Setup(x => x.HasSharedLearningSessionAsync(_requesterId, _mentorId))
            .ReturnsAsync(false);

        var action = () => _service.CreateAsync(_requesterId, new CreateSkillVerificationRequest
        {
            MentorUserId = _mentorId,
            SkillId = _skillId
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only users with at least one shared learning session can request skill verification.");
    }

    [Fact]
    public async Task UpdateAsync_WhenAccepted_ShouldSetGeneratedScore()
    {
        var requester = ServiceTestData.User(_requesterId);
        var mentor = ServiceTestData.User(_mentorId);
        var skill = new Skill { Id = _skillId, Name = "C#" };
        var request = new SkillVerificationRequest
        {
            Id = Guid.NewGuid(),
            RequesterUserId = _requesterId,
            MentorUserId = _mentorId,
            SkillId = _skillId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RequesterUser = requester,
            MentorUser = mentor,
            Skill = skill
        };

        _repository.Setup(x => x.GetUserByIdAsync(_mentorId))
            .ReturnsAsync(mentor);
        _repository.Setup(x => x.GetRequestByIdAsync(request.Id))
            .ReturnsAsync(request);

        var result = await _service.UpdateAsync(_mentorId, request.Id, new UpdateSkillVerificationRequest
        {
            Status = "Accepted",
            Note = "Good work."
        });

        result.Status.Should().Be("Accepted");
        result.Score.Should().Be(5);
        result.Note.Should().Be("Good work.");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
    }
}
