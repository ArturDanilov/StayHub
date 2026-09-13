using Microsoft.Extensions.Options;

namespace StayHub.Api.Synchronization;

public sealed class ReservationSynchronizationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AutomaticSynchronizationOptions> options,
    ILogger<ReservationSynchronizationWorker> logger)
    : BackgroundService
{
    private readonly AutomaticSynchronizationOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Automatic reservation synchronization is disabled.");
            return;
        }

        logger.LogInformation(
            "Automatic reservation synchronization is enabled with an interval of {IntervalMinutes} minutes.",
            _options.IntervalMinutes);

        try
        {
            if (_options.InitialDelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_options.InitialDelaySeconds), stoppingToken);

            await RunOnceAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RunOnceAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Automatic reservation synchronization stopped.");
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var job = scope.ServiceProvider.GetRequiredService<AutomaticSynchronizationJob>();
        await job.RunAsync(cancellationToken);
    }
}
