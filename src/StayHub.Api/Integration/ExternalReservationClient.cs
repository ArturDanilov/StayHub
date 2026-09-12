using System.Net.Http.Json;
using StayHub.Business.Interfaces;
using StayHub.Contracts.Synchronization;

namespace StayHub.Api.Integration;

public sealed class ExternalReservationClient(HttpClient httpClient)
    : IExternalReservationClient
{
    public async Task<IReadOnlyList<ExternalReservationResponse>> GetReservationsAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        var baseUri = new Uri(sourceUrl.TrimEnd('/') + '/', UriKind.Absolute);
        var endpoint = new Uri(baseUri, "api/reservations");

        using var response = await httpClient.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ExternalReservationResponse>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }
}
