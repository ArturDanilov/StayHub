using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface IReservationsService
{
    Task<IReadOnlyList<ReservationOverview>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, ReservationStatusContract status, CancellationToken cancellationToken = default);
}
