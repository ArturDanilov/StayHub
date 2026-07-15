using PulseHub.Domain.Models;

namespace PulseHub.Business.Interfaces;

public interface ISourceRepository
{
    Task<IReadOnlyList<Source>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Source?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Source> AddAsync(
        Source source,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    void Delete(Source source);
}
