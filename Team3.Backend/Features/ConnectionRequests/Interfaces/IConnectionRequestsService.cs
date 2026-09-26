using Team3.Backend.Features.ConnectionRequests.Dtos;

namespace Team3.Backend.Features.ConnectionRequests.Interfaces;

public interface IConnectionRequestsService
{
    Task<ConnectionRequestResponse> SendAsync(
        string firebaseUid,
        SendConnectionRequestRequest request);

    Task<List<ConnectionRequestResponse>> GetReceivedAsync(
        string firebaseUid);

    Task<List<ConnectionRequestResponse>> GetSentAsync(
        string firebaseUid);

    Task<ConnectionRequestResponse> UpdateStatusAsync(
        string firebaseUid,
        Guid connectionRequestId,
        UpdateConnectionRequestStatusRequest request);
}
