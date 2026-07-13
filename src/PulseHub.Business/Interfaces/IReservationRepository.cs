using PulseHub.Domain.Models;

namespace PulseHub.Business.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExternalIdExistsAsync(
        string externalId,
        CancellationToken cancellationToken = default);

    Task<Reservation> AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    void Delete(Reservation reservation);
    
}
