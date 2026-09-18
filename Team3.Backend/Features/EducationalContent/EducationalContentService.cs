using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.EducationalContent;

public class EducationalContentService : IEducationalContentService
{
    private readonly IEducationalContentRepository _educationalContentRepository;

    public EducationalContentService(
        IEducationalContentRepository educationalContentRepository)
    {
        _educationalContentRepository = educationalContentRepository;
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
            UpdatedAt = DateTime.UtcNow
        };

        _educationalContentRepository.Add(content);
        await _educationalContentRepository.SaveChangesAsync();

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

        if (request.Title is not null)
        {
            ValidateTitle(request.Title);
            content.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            content.Description = request.Description;
        }

        if (request.ContentType is not null)
        {
            ValidateContentType(request.ContentType);
            content.ContentType = request.ContentType.Trim();
        }

        if (request.ContentUrl is not null)
        {
            content.ContentUrl = request.ContentUrl;
        }

        content.UpdatedAt = DateTime.UtcNow;

        await _educationalContentRepository.SaveChangesAsync();

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
}