using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class ProfileSyncClient : AiHttpClient, IProfileSyncClient
{
    public ProfileSyncClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<ProfileSyncClient> logger)
        : base(httpClient, options, logger)
    {
    }

    public Task<ProfileSyncResponse> SyncAsync(
        ProfileSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operationId = $"profile_sync_{request.SyncType}_{Guid.NewGuid():N}";

        return PostAsync<ProfileSyncRequest, ProfileSyncResponse>(
            "/api/v1/ai/recommendations/people/sync",
            request,
            operationId,
            cancellationToken);
    }
}
