using StayHub.Domain.Models;
using StayHub.Business.Models;

namespace StayHub.Business.Interfaces;

public interface IReservationRepository
{
    Task<PagedResult<Reservation>> GetAllAsync(
        ReservationQuery query,
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
