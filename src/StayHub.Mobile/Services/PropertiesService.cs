using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayHub.Contracts.Properties;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public sealed class PropertiesService(
    HttpClient httpClient,
    IAuthService authService) : IPropertiesService
{
    public async Task<IReadOnlyList<PropertyOverview>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var token = await authService.GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException();

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/properties");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();

            if (!response.IsSuccessStatusCode)
                throw new ApiException("Could not load properties from StayHub API.");

            var properties = await response.Content.ReadFromJsonAsync<IReadOnlyList<PropertyResponse>>(
                                 cancellationToken: cancellationToken)
                             ?? [];

            return properties
                .Select(property => new PropertyOverview(
                    property.Id,
                    property.Name,
                    $"{property.City}, {property.CountryCode}",
                    $"Added {property.CreatedAtUtc.ToLocalTime():d MMM yyyy}"))
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to StayHub API. Start the API and try again.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("StayHub API did not respond in time.");
        }
    }
}
