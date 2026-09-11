using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class PropertyRepository(StayHubDbContext dbContext)
    : IPropertyRepository
{
    public async Task<IReadOnlyList<Property>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Property?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Properties
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Property?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Properties.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<Property> AddAsync(
        Property property,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Properties.AddAsync(property, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return property;
    }

    public void Delete(Property property)
    {
        dbContext.Properties.Remove(property);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
