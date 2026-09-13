using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StayHub.Contracts.Reservations;
using StayHub.Mobile.Models;
using StayHub.Contracts.Common;

namespace StayHub.Mobile.Services;

public sealed class ReservationsService(
    HttpClient httpClient,
    IAuthService authService) : IReservationsService
{
    public async Task<PagedReservationOverview> GetAllAsync(
        ReservationSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, BuildQueryUri(criteria));

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Could not load bookings.");

            var reservations = await response.Content.ReadFromJsonAsync<PagedResponse<ReservationResponse>>(
                                   cancellationToken: cancellationToken)
                               ?? new PagedResponse<ReservationResponse>([], 1, criteria.PageSize, 0);

            var items = reservations.Items
                .Select(Map)
                .ToList();
            return new PagedReservationOverview(
                items,
                reservations.Page,
                reservations.PageSize,
                reservations.TotalCount,
                reservations.TotalPages);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            || exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw CreateConnectionException(exception, cancellationToken);
        }
    }

    private static string BuildQueryUri(ReservationSearchCriteria criteria)
    {
        var values = new List<string>
        {
            $"page={criteria.Page}",
            $"pageSize={criteria.PageSize}",
            $"sortBy={criteria.SortBy}",
            $"sortDirection={criteria.SortDirection}"
        };

        if (!string.IsNullOrWhiteSpace(criteria.Search))
            values.Add($"search={Uri.EscapeDataString(criteria.Search.Trim())}");
        if (criteria.Status.HasValue)
            values.Add($"status={criteria.Status.Value}");
        if (criteria.ArrivalFrom.HasValue)
            values.Add($"arrivalFrom={criteria.ArrivalFrom.Value:yyyy-MM-dd}");
        if (criteria.ArrivalTo.HasValue)
            values.Add($"arrivalTo={criteria.ArrivalTo.Value:yyyy-MM-dd}");
        if (criteria.PropertyId.HasValue)
            values.Add($"propertyId={criteria.PropertyId.Value}");
        if (criteria.SourceId.HasValue)
            values.Add($"sourceId={criteria.SourceId.Value}");

        return $"api/reservations?{string.Join('&', values)}";
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
            await EnsureSuccessAsync(response, "Could not update the booking status.");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw CreateConnectionException(exception, cancellationToken);
        }
    }

    public async Task CreateAsync(
        CreateReservationRequest reservation,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/reservations");
        request.Content = JsonContent.Create(reservation);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Could not create the booking.");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw CreateConnectionException(exception, cancellationToken);
        }
    }

    public async Task UpdateAsync(
        int id,
        UpdateReservationRequest reservation,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/reservations/{id}");
        request.Content = JsonContent.Create(reservation);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Could not update the booking.");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw CreateConnectionException(exception, cancellationToken);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/reservations/{id}");

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Could not delete the booking.");
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
        throw new ApiException(GetErrorMessage(serverMessage, fallbackMessage));
    }

    private static string GetErrorMessage(string responseBody, string fallbackMessage)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return fallbackMessage;

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array
                        && property.Value.GetArrayLength() > 0)
                        return property.Value[0].GetString() ?? fallbackMessage;
                }
            }

            if (document.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? fallbackMessage;
        }
        catch (JsonException)
        {
            // The API also returns plain-text business errors.
        }

        return responseBody.Trim('"');
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
            reservation.SourceId,
            reservation.SourceName,
            reservation.Guest.Id,
            $"{reservation.Guest.FirstName} {reservation.Guest.LastName}",
            reservation.Guest.Email,
            reservation.Guest.Phone,
            reservation.PropertyId,
            reservation.PropertyName,
            reservation.ArrivalDate,
            reservation.DepartureDate,
            reservation.Status,
            reservation.CreatedAtUtc);
    }
}
