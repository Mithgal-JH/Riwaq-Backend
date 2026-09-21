using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Team3.Backend.Features.AI;

public abstract class AiHttpClient
{
    private const int MaxAttempts = 3;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    protected AiHttpClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        Options = options.Value;
    }

    protected AiOptions Options { get; }

    protected async Task<TResponse> GetAsync<TResponse>(
        string path,
        string operationId,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(
                    httpRequest,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (
                !cancellationToken.IsCancellationRequested && attempt < MaxAttempts)
            {
                LogRetry(operationId, attempt, "request timeout");
                await DelayBeforeRetryAsync(attempt, cancellationToken);
                continue;
            }
            catch (HttpRequestException exception) when (attempt < MaxAttempts)
            {
                LogRetry(operationId, attempt, exception.Message);
                await DelayBeforeRetryAsync(attempt, cancellationToken);
                continue;
            }
            catch (OperationCanceledException) when (
                !cancellationToken.IsCancellationRequested)
            {
                throw CreateUnavailableException(operationId, "request timeout");
            }
            catch (HttpRequestException exception)
            {
                throw CreateUnavailableException(
                    operationId,
                    "request failed",
                    exception);
            }

            using (response)
            {
                var responseBody = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw CreateHttpException(
                        operationId,
                        response.StatusCode,
                        responseBody);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw CreateHttpException(
                        operationId,
                        response.StatusCode,
                        responseBody);
                }

                if (response.StatusCode == HttpStatusCode.InternalServerError
                    && attempt < MaxAttempts)
                {
                    LogRetry(operationId, attempt, "HTTP 500");
                    await DelayBeforeRetryAsync(attempt, cancellationToken);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw CreateHttpException(
                        operationId,
                        response.StatusCode,
                        responseBody);
                }

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(
                        responseBody,
                        SerializerOptions)
                        ?? throw new AiServiceException(
                            "AI service returned an empty response.",
                            operationId,
                            response.StatusCode,
                            responseBody);
                }
                catch (JsonException exception)
                {
                    throw new AiServiceException(
                        "AI service returned an invalid response.",
                        operationId,
                        response.StatusCode,
                        responseBody,
                        exception);
                }
            }
        }

        throw CreateUnavailableException(operationId, "request failed after retries");
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        string operationId,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var payload = JsonSerializer.Serialize(request, SerializerOptions);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(
                    payload,
                    Encoding.UTF8,
                    "application/json")
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(
                    httpRequest,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (
                !cancellationToken.IsCancellationRequested && attempt < MaxAttempts)
            {
                LogRetry(operationId, attempt, "request timeout");
                await DelayBeforeRetryAsync(attempt, cancellationToken);
                continue;
            }
            catch (HttpRequestException exception) when (attempt < MaxAttempts)
            {
                LogRetry(operationId, attempt, exception.Message);
                await DelayBeforeRetryAsync(attempt, cancellationToken);
                continue;
            }
            catch (OperationCanceledException) when (
                !cancellationToken.IsCancellationRequested)
            {
                throw CreateUnavailableException(operationId, "request timeout");
            }
            catch (HttpRequestException exception)
            {
                throw CreateUnavailableException(
                    operationId,
                    "request failed",
                    exception);
            }

            using (response)
            {
                var responseBody = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.BadRequest
                    || response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    throw CreateHttpException(
                        operationId,
                        response.StatusCode,
                        responseBody);
                }

                if (response.StatusCode == HttpStatusCode.InternalServerError
                    && attempt < MaxAttempts)
                {
                    LogRetry(operationId, attempt, "HTTP 500");
                    await DelayBeforeRetryAsync(attempt, cancellationToken);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw CreateHttpException(
                        operationId,
                        response.StatusCode,
                        responseBody);
                }

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(
                        responseBody,
                        SerializerOptions)
                        ?? throw new AiServiceException(
                            "AI service returned an empty response.",
                            operationId,
                            response.StatusCode,
                            responseBody);
                }
                catch (JsonException exception)
                {
                    throw new AiServiceException(
                        "AI service returned an invalid response.",
                        operationId,
                        response.StatusCode,
                        responseBody,
                        exception);
                }
            }
        }

        throw CreateUnavailableException(operationId, "request failed after retries");
    }

    private void EnsureConfigured()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException(
                "AI:BaseUrl must be configured before using AI integration clients.");
        }
    }

    private async Task DelayBeforeRetryAsync(
        int attempt,
        CancellationToken cancellationToken)
    {
        await Task.Delay(
            TimeSpan.FromMilliseconds(100 * attempt),
            cancellationToken);
    }

    private void LogRetry(string operationId, int attempt, string reason)
    {
        _logger.LogWarning(
            "Retrying AI operation {OperationId} after attempt {Attempt}. Reason: {Reason}.",
            operationId,
            attempt,
            reason);
    }

    private static AiServiceException CreateHttpException(
        string operationId,
        HttpStatusCode statusCode,
        string responseBody)
    {
        return new AiServiceException(
            "AI service rejected or failed the request.",
            operationId,
            statusCode,
            responseBody);
    }

    private static AiServiceException CreateUnavailableException(
        string operationId,
        string reason,
        Exception? exception = null)
    {
        return new AiServiceException(
            $"AI service is unavailable: {reason}.",
            operationId,
            innerException: exception);
    }
}
