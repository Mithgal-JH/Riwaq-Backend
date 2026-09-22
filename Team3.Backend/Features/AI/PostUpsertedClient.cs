using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class PostUpsertedClient : AiHttpClient, IPostUpsertedClient
{
    public PostUpsertedClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<PostUpsertedClient> logger)
        : base(httpClient, options, logger)
    {
    }

    public Task<PostUpsertedResponse> UpsertAsync(
        PostUpsertedRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return PostAsync<PostUpsertedRequest, PostUpsertedResponse>(
            "/api/v1/events/post-upserted",
            request,
            $"op_{Guid.NewGuid():N}",
            cancellationToken);
    }
}
