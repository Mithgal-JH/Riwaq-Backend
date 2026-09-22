using EducationalContentModel = Team3.Backend.Models.EducationalContent;
using Team3.Backend.Models;

namespace Team3.Backend.Features.EducationalContent.Interfaces;

public interface IEducationalContentRepository
{
    Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);

    Task<List<EducationalContentModel>> GetAllAsync();

    Task<EducationalContentModel?> GetByIdAsync(Guid contentId);

    Task<EducationalContentModel?> GetByIdForUserAsync(
        Guid contentId,
        Guid userId);

    void Add(EducationalContentModel content);

    void Remove(EducationalContentModel content);

    Task SaveChangesAsync();
}