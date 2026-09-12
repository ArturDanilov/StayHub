using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface ISynchronizationService
{
    Task<IReadOnlyList<SynchronizationSourceOption>> GetSourcesAsync(
        CancellationToken cancellationToken = default);

    Task<SynchronizationRunOverview> SynchronizeAsync(
        int sourceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SynchronizationRunOverview>> GetRecentRunsAsync(
        int take = 20,
        CancellationToken cancellationToken = default);
}
