using StayHub.Contracts.Synchronization;

namespace StayHub.Business.Interfaces;

public interface IExternalReservationClient
{
    Task<IReadOnlyList<ExternalReservationResponse>> GetReservationsAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default);
}
