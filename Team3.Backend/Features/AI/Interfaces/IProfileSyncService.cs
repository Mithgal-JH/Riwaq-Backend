namespace Team3.Backend.Features.AI.Interfaces;

public interface IProfileSyncService
{
    Task UpsertProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task FullSyncAsync(
        CancellationToken cancellationToken = default);

    Task DeleteProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
