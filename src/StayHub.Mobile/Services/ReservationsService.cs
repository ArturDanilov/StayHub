using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public sealed class ReservationsService(
    HttpClient httpClient,
    IAuthService authService) : IReservationsService
{
    public async Task<IReadOnlyList<ReservationOverview>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/reservations");

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Could not load reservations.");

            var reservations = await response.Content.ReadFromJsonAsync<IReadOnlyList<ReservationResponse>>(
                                   cancellationToken: cancellationToken)
                               ?? [];

            return reservations
                .Select(Map)
                .OrderBy(reservation => reservation.ArrivalDate)
                .ToList();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw CreateConnectionException(exception, cancellationToken);
        }
    }

    public async Task UpdateStatusAsync(
        int id,
        ReservationStatusContract status,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Patch,
            $"api/reservations/{id}/status");
        request.Content = JsonContent.Create(new UpdateReservationStatusRequest(status));

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Could not update the reservation status.");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw CreateConnectionException(exception, cancellationToken);
        }
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string uri)
    {
        var token = await authService.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException();

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallbackMessage)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new ApiException("Your role does not allow this action.");

        if (response.IsSuccessStatusCode)
            return;

        var serverMessage = await response.Content.ReadAsStringAsync();
        throw new ApiException(string.IsNullOrWhiteSpace(serverMessage)
            ? fallbackMessage
            : serverMessage.Trim('"'));
    }

    private static ApiException CreateConnectionException(
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            return new ApiException("StayHub API did not respond in time.");

        return new ApiException("Cannot connect to StayHub API. Start the API and try again.");
    }

    private static ReservationOverview Map(ReservationResponse reservation)
    {
        return new ReservationOverview(
            reservation.Id,
            reservation.ExternalId,
            reservation.SourceName,
            $"{reservation.Guest.FirstName} {reservation.Guest.LastName}",
            reservation.Guest.Email,
            reservation.Guest.Phone,
            reservation.PropertyName,
            reservation.ArrivalDate,
            reservation.DepartureDate,
            reservation.Status,
            reservation.CreatedAtUtc);
    }
}
