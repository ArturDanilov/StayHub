using StayHub.Contracts.Guests;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface IReservationFormService
{
    Task<ReservationFormOptions> GetOptionsAsync(CancellationToken cancellationToken = default);
    Task<GuestOption> CreateGuestAsync(CreateGuestRequest request, CancellationToken cancellationToken = default);
}
