using FluentAssertions;
using Moq;
using Team3.Backend.Features.EducationalContent;
using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class EducationalContentServiceTests
{
    private readonly Mock<IEducationalContentRepository> _repository = new();
    private readonly EducationalContentService _service;

    public EducationalContentServiceTests()
    {
        _service = new EducationalContentService(_repository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEducationalContent()
    {
        var firstContent = CreateContent();
        var secondContent = CreateContent();

        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<EducationalContent>
            {
                firstContent,
                secondContent
            });

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Id == firstContent.Id);
        result.Should().Contain(x => x.Id == secondContent.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnContent_WhenContentExists()
    {
        var content = CreateContent();

        _repository
            .Setup(x => x.GetByIdAsync(content.Id))
            .ReturnsAsync(content);

        var result = await _service.GetByIdAsync(content.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(content.Id);
        result.Title.Should().Be(content.Title);
        result.ContentType.Should().Be(content.ContentType);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        var action = () => _service.GetByIdAsync(Guid.Empty);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Educational content ID must be a valid Guid.");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateContent_WhenRequestIsValid()
    {
        var user = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        var request = new CreateEducationalContentRequest
        {
            Title = "  C# Basics  ",
            Description = "Introduction to C#.",
            ContentType = "  Video  ",
            ContentUrl = "https://example.com/video"
        };

        var result = await _service.CreateAsync(
            "firebase-uid",
            request);

        result.UserId.Should().Be(user.Id);
        result.Title.Should().Be("C# Basics");
        result.Description.Should().Be("Introduction to C#.");
        result.ContentType.Should().Be("Video");
        result.ContentUrl.Should().Be("https://example.com/video");

        // Verify that the content was added to the repository.
        _repository.Verify(
            x => x.Add(It.Is<EducationalContent>(content =>
                content.UserId == user.Id &&
                content.Title == "C# Basics" &&
                content.ContentType == "Video")),
            Times.Once);

        // Verify that the changes were saved.
        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTitleIsEmpty()
    {
        var request = new CreateEducationalContentRequest
        {
            Title = "   ",
            ContentType = "Video"
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            request);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Title is required.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenContentTypeIsEmpty()
    {
        var request = new CreateEducationalContentRequest
        {
            Title = "C# Basics",
            ContentType = "   "
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            request);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Content type is required.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenContentTypeIsNotAllowed()
    {
        var request = new CreateEducationalContentRequest
        {
            Title = "C# Basics",
            ContentType = "Audio"
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            request);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage(
                "Content type must be Text, Image, Video, File, or ExternalLink.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync((User?)null);

        var request = new CreateEducationalContentRequest
        {
            Title = "C# Basics",
            ContentType = "Video"
        };

        var action = () => _service.CreateAsync(
            "firebase-uid",
            request);

        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateContent_WhenContentBelongsToUser()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var content = CreateContent(user.Id);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdForUserAsync(content.Id, user.Id))
            .ReturnsAsync(content);

        var request = new UpdateEducationalContentRequest
        {
            Title = "  Updated Title  ",
            Description = "Updated description.",
            ContentType = "Image",
            ContentUrl = "https://example.com/image"
        };

        var result = await _service.UpdateAsync(
            "firebase-uid",
            content.Id,
            request);

        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated description.");
        result.ContentType.Should().Be("Image");
        result.ContentUrl.Should().Be("https://example.com/image");

        // Verify that the updated entity was saved.
        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenContentDoesNotExistForUser()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var contentId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdForUserAsync(contentId, user.Id))
            .ReturnsAsync((EducationalContent?)null);

        var request = new UpdateEducationalContentRequest
        {
            Title = "Updated Title"
        };

        var action = () => _service.UpdateAsync(
            "firebase-uid",
            contentId,
            request);

        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Educational content not found.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteContent_WhenContentBelongsToUser()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var content = CreateContent(user.Id);

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdForUserAsync(content.Id, user.Id))
            .ReturnsAsync(content);

        await _service.DeleteAsync(
            "firebase-uid",
            content.Id);

        // Verify that the correct content was removed.
        _repository.Verify(
            x => x.Remove(content),
            Times.Once);

        // Verify that the deletion was saved.
        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenContentDoesNotExistForUser()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var contentId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdForUserAsync(contentId, user.Id))
            .ReturnsAsync((EducationalContent?)null);

        var action = () => _service.DeleteAsync(
            "firebase-uid",
            contentId);

        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Educational content not found.");
    }

    private static EducationalContent CreateContent(
        Guid? userId = null)
    {
        return new EducationalContent
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Title = "C# Basics",
            Description = "Introduction to C#.",
            ContentType = "Video",
            ContentUrl = "https://example.com/video",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}