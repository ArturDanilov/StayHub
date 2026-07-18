using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class SourceRepository(StayHubDbContext dbContext)
    : ISourceRepository
{
    public async Task<IReadOnlyList<Source>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Sources
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Source?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Sources
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Source> AddAsync(
        Source source,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Sources.AddAsync(source, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return source;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Delete(Source source)
    {
        dbContext.Sources.Remove(source);
    }
}
