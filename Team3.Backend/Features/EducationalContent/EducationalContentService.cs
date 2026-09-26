using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.EducationalContent;

public class EducationalContentService : IEducationalContentService
{
    private readonly IEducationalContentRepository _educationalContentRepository;
    private readonly IContentAnalysisService? _contentAnalysisService;

    public EducationalContentService(
        IEducationalContentRepository educationalContentRepository,
        IContentAnalysisService? contentAnalysisService = null)
    {
        _educationalContentRepository = educationalContentRepository;
        _contentAnalysisService = contentAnalysisService;
    }

    public async Task<IReadOnlyList<EducationalContentResponse>> GetAllAsync()
    {
        var content = await _educationalContentRepository.GetAllAsync();

        return content.Select(MapToResponse).ToList();
    }

    public async Task<EducationalContentResponse?> GetByIdAsync(
        Guid contentId)
    {
        ValidateId(contentId);

        var content = await _educationalContentRepository.GetByIdAsync(
            contentId);

        return content is null ? null : MapToResponse(content);
    }

    public async Task<EducationalContentResponse> CreateAsync(
        string firebaseUid,
        CreateEducationalContentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateTitle(request.Title);
        ValidateContentType(request.ContentType);

        var user = await GetUserAsync(firebaseUid);

        var content = new EducationalContentModel
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Title = request.Title.Trim(),
            Description = request.Description,
            ContentType = request.ContentType.Trim(),
            ContentUrl = request.ContentUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ContentVersion = 1
        };

        _educationalContentRepository.Add(content);
        await _educationalContentRepository.SaveChangesAsync();
        await AnalyzeAsync(content);

        return MapToResponse(content);
    }

    public async Task<EducationalContentResponse> UpdateAsync(
        string firebaseUid,
        Guid contentId,
        UpdateEducationalContentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(contentId);

        var user = await GetUserAsync(firebaseUid);

        var content = await _educationalContentRepository.GetByIdForUserAsync(
            contentId,
            user.Id);

        if (content is null)
        {
            throw new KeyNotFoundException("Educational content not found.");
        }

        var contentChanged = false;

        if (request.Title is not null)
        {
            ValidateTitle(request.Title);
            var title = request.Title.Trim();
            contentChanged |= content.Title != title;
            content.Title = title;
        }

        if (request.Description is not null)
        {
            contentChanged |= content.Description != request.Description;
            content.Description = request.Description;
        }

        if (request.ContentType is not null)
        {
            ValidateContentType(request.ContentType);
            var contentType = request.ContentType.Trim();
            contentChanged |= content.ContentType != contentType;
            content.ContentType = contentType;
        }

        if (request.ContentUrl is not null)
        {
            contentChanged |= content.ContentUrl != request.ContentUrl;
            content.ContentUrl = request.ContentUrl;
        }

        if (contentChanged)
        {
            content.ContentVersion++;
        }

        content.UpdatedAt = DateTime.UtcNow;

        await _educationalContentRepository.SaveChangesAsync();
        if (contentChanged)
        {
            await AnalyzeAsync(content);
        }

        return MapToResponse(content);
    }

    public async Task DeleteAsync(
        string firebaseUid,
        Guid contentId)
    {
        ValidateId(contentId);

        var user = await GetUserAsync(firebaseUid);

        var content = await _educationalContentRepository.GetByIdForUserAsync(
            contentId,
            user.Id);

        if (content is null)
        {
            throw new KeyNotFoundException("Educational content not found.");
        }

        _educationalContentRepository.Remove(content);
        await _educationalContentRepository.SaveChangesAsync();
    }

    private async Task<User> GetUserAsync(string firebaseUid)
    {
        var user = await _educationalContentRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        return user ?? throw new KeyNotFoundException("User not found.");
    }

    private static void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }
    }

    private static void ValidateContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.");
        }

        var allowedContentTypes = new[]
        {
        "Text",
        "Image",
        "Video",
        "File",
        "ExternalLink"
    };

        if (!allowedContentTypes.Contains(contentType.Trim()))
        {
            throw new ArgumentException(
                "Content type must be Text, Image, Video, File, or ExternalLink.");
        }
    }

    private static void ValidateId(Guid contentId)
    {
        if (contentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Educational content ID must be a valid Guid.");
        }
    }

    private static EducationalContentResponse MapToResponse(
        EducationalContentModel content)
    {
        return new EducationalContentResponse
        {
            Id = content.Id,
            UserId = content.UserId,
            Title = content.Title,
            Description = content.Description,
            ContentType = content.ContentType,
            ContentUrl = content.ContentUrl,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }

    private async Task AnalyzeAsync(EducationalContentModel content)
    {
        if (_contentAnalysisService is not null)
        {
            await _contentAnalysisService.AnalyzeAsync(content);
        }
    }
}