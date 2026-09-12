using StayHub.Domain.Models;

namespace StayHub.Business.Interfaces;

public interface IPropertyRepository
{
    Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Property?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Property?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<Property> AddAsync(
        Property property,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Delete(Property property);
}
