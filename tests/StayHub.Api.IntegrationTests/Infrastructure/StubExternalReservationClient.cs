using StayHub.Business.Interfaces;
using StayHub.Contracts.Synchronization;

namespace StayHub.Api.IntegrationTests.Infrastructure;

public sealed class StubExternalReservationClient : IExternalReservationClient
{
    public IReadOnlyList<ExternalReservationResponse> Reservations { get; set; } = [];

    public Task<IReadOnlyList<ExternalReservationResponse>> GetReservationsAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Reservations);
    }
}
