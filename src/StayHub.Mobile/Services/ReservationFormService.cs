using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayHub.Contracts.Guests;
using StayHub.Contracts.Properties;
using StayHub.Contracts.Sources;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public sealed class ReservationFormService(HttpClient httpClient, IAuthService authService)
    : IReservationFormService
{
    public async Task<ReservationFormOptions> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var properties = await GetAsync<IReadOnlyList<PropertyResponse>>("api/properties", cancellationToken) ?? [];
        var guests = await GetAsync<IReadOnlyList<GuestResponse>>("api/guests", cancellationToken) ?? [];
        var sources = await GetAsync<IReadOnlyList<SourceResponse>>("api/sources", cancellationToken) ?? [];

        return new ReservationFormOptions(
            properties.Select(item => new PropertyOption(item.Id, item.Name)).OrderBy(item => item.Name).ToList(),
            guests.Select(MapGuest).OrderBy(item => item.Name).ToList(),
            sources.Where(item => item.IsEnabled).Select(item => new SourceOption(item.Id, item.Name)).OrderBy(item => item.Name).ToList());
    }

    public async Task<GuestOption> CreateGuestAsync(
        CreateGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/guests");
        message.Content = JsonContent.Create(request);
        using var response = await SendAsync(message, cancellationToken);
        var guest = await response.Content.ReadFromJsonAsync<GuestResponse>(cancellationToken: cancellationToken)
                    ?? throw new ApiException("StayHub API returned an empty guest response.");
        return MapGuest(guest);
    }

    private async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        using var message = await CreateAuthorizedRequestAsync(HttpMethod.Get, uri);
        using var response = await SendAsync(message, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                throw new UnauthorizedAccessException();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                response.Dispose();
                throw new ApiException("Your role does not allow this action.");
            }

            if (response.IsSuccessStatusCode)
                return response;

            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            response.Dispose();
            throw new ApiException(string.IsNullOrWhiteSpace(message) ? "StayHub API rejected the request." : message.Trim('"'));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException(exception is TaskCanceledException && !cancellationToken.IsCancellationRequested
                ? "StayHub API did not respond in time."
                : "Cannot connect to StayHub API. Start the API and try again.");
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

    private static GuestOption MapGuest(GuestResponse guest) =>
        new(guest.Id, $"{guest.FirstName} {guest.LastName}", guest.Email);
}
