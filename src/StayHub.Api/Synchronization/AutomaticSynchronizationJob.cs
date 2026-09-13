using Microsoft.Extensions.Options;
using StayHub.Business.Interfaces;

namespace StayHub.Api.Synchronization;

public sealed class AutomaticSynchronizationJob(
    ISourceRepository sourceRepository,
    ISynchronizationManager synchronizationManager,
    IOptions<AutomaticSynchronizationOptions> options,
    ILogger<AutomaticSynchronizationJob> logger)
{
    private readonly AutomaticSynchronizationOptions _options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var configuredNames = _options.SourceNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sources = await sourceRepository.GetAllAsync(cancellationToken);
        var eligibleSources = sources.Where(source =>
            source.IsEnabled
            && !string.IsNullOrWhiteSpace(source.Url)
            && (configuredNames.Count == 0 || configuredNames.Contains(source.Name)));

        foreach (var source in eligibleSources)
        {
            try
            {
                var result = await synchronizationManager.SynchronizeAsync(source.Id, cancellationToken);
                if (result.IsSuccess)
                {
                    logger.LogInformation(
                        "Automatic synchronization for source {SourceName} completed with status {Status}",
                        source.Name,
                        result.Value?.Status ?? "Unknown");
                }
                else
                {
                    logger.LogWarning(
                        "Automatic synchronization for source {SourceName} was skipped or rejected: {Error}",
                        source.Name,
                        result.Error);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unexpected error during automatic synchronization for source {SourceName}",
                    source.Name);
            }
        }
    }
}
