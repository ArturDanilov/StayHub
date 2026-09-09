using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface IReservationsService
{
    Task<PagedReservationOverview> GetAllAsync(
        ReservationSearchCriteria criteria,
        CancellationToken cancellationToken = default);
    Task CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateReservationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, ReservationStatusContract status, CancellationToken cancellationToken = default);
}
