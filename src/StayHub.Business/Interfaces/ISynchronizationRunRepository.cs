using StayHub.Domain.Models;

namespace StayHub.Business.Interfaces;

public interface ISynchronizationRunRepository
{
    Task<IReadOnlyList<SynchronizationRun>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<SynchronizationRun> AddAsync(
        SynchronizationRun run,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
