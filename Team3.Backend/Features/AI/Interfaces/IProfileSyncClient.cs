using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IProfileSyncClient
{
    Task<ProfileSyncResponse> SyncAsync(
        ProfileSyncRequest request,
        CancellationToken cancellationToken = default);
}
