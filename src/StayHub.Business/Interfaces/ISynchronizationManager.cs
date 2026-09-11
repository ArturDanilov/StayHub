using StayHub.Business.Results;
using StayHub.Contracts.Synchronization;

namespace StayHub.Business.Interfaces;

public interface ISynchronizationManager
{
    Task<OperationResult<SynchronizationRunResponse, SynchronizationError>> SynchronizeAsync(
        int sourceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SynchronizationRunResponse>> GetRecentRunsAsync(
        int take = 20,
        CancellationToken cancellationToken = default);
}
