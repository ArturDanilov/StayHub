using PulseHub.Domain.Models;

namespace PulseHub.Business.Repositories;

public interface IPropertyRepository
{
    Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Property?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Property> AddAsync(
        Property property,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Delete(Property property);
}
