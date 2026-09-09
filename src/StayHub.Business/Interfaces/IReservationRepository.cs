using StayHub.Domain.Models;

namespace StayHub.Business.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExternalIdExistsAsync(
        int sourceId,
        string externalId,
        int? excludedReservationId = null,
        CancellationToken cancellationToken = default);

    Task<Reservation> AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    void Delete(Reservation reservation);
    
}
