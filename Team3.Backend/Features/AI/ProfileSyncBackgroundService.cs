using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Team3.Backend.Features.AI.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class ProfileSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProfileSyncBackgroundService> _logger;
    private readonly TimeSpan _startupDelay = TimeSpan.FromSeconds(15);
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public ProfileSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ProfileSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(_startupDelay, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        using var timer = new PeriodicTimer(_syncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunFullSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Full profile synchronization loop failed.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RunFullSyncAsync(CancellationToken cancellationToken)
    {
        if (!await _syncLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogDebug(
                "Skipping full profile synchronization because another synchronization is already running.");
            return;
        }

        try
        {
            _logger.LogInformation("Starting full profile synchronization cycle.");

            using var scope = _scopeFactory.CreateScope();
            var profileSyncService = scope.ServiceProvider
                .GetRequiredService<IProfileSyncService>();

            await profileSyncService.FullSyncAsync(cancellationToken);

            _logger.LogInformation("Completed full profile synchronization cycle.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Full profile synchronization timed out or was interrupted before completion.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Full profile synchronization cycle failed.");
        }
        finally
        {
            _syncLock.Release();
        }
    }
}
