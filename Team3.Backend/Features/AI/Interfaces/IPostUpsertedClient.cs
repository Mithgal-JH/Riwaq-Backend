using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IPostUpsertedClient
{
    Task<PostUpsertedResponse> UpsertAsync(
        PostUpsertedRequest request,
        CancellationToken cancellationToken = default);
}
