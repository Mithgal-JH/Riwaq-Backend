using System.Net;

namespace Team3.Backend.Features.AI;

public sealed class AiServiceException : Exception
{
    public AiServiceException(
        string message,
        string operationId,
        HttpStatusCode? statusCode = null,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        OperationId = operationId;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public string OperationId { get; }

    public HttpStatusCode? StatusCode { get; }

    public string? ResponseBody { get; }
}
