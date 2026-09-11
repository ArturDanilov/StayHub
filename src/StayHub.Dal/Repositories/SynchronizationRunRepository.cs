using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class SynchronizationRunRepository(StayHubDbContext dbContext)
    : ISynchronizationRunRepository
{
    public async Task<IReadOnlyList<SynchronizationRun>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.SynchronizationRuns
            .AsNoTracking()
            .Include(x => x.Source)
            .OrderByDescending(x => x.StartedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<SynchronizationRun> AddAsync(
        SynchronizationRun run,
        CancellationToken cancellationToken = default)
    {
        await dbContext.SynchronizationRuns.AddAsync(run, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return run;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
