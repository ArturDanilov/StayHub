using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayHub.Contracts.Sources;
using StayHub.Contracts.Synchronization;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public sealed class SynchronizationService(HttpClient httpClient, IAuthService authService)
    : ISynchronizationService
{
    private const string SupportedSourceName = "Mock PMS";

    public async Task<IReadOnlyList<SynchronizationSourceOption>> GetSourcesAsync(
        CancellationToken cancellationToken = default)
    {
        var sources = await GetAsync<IReadOnlyList<SourceResponse>>("api/sources", cancellationToken) ?? [];
        return sources
            .Where(source => source.IsEnabled
                             && !string.IsNullOrWhiteSpace(source.Url)
                             && string.Equals(
                                 source.Name,
                                 SupportedSourceName,
                                 StringComparison.OrdinalIgnoreCase))
            .Select(source => new SynchronizationSourceOption(source.Id, source.Name))
            .OrderBy(source => source.Name)
            .ToList();
    }

    public async Task<SynchronizationRunOverview> SynchronizeAsync(
        int sourceId,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Post,
            $"api/synchronization/sources/{sourceId}");
        using var response = await SendAsync(request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<SynchronizationRunResponse>(
                         cancellationToken: cancellationToken)
                     ?? throw new ApiException("StayHub API returned an empty synchronization response.");
        return SynchronizationRunOverview.FromResponse(result);
    }

    public async Task<IReadOnlyList<SynchronizationRunOverview>> GetRecentRunsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var runs = await GetAsync<IReadOnlyList<SynchronizationRunResponse>>(
                       $"api/synchronization/runs?take={take}",
                       cancellationToken) ?? [];
        return runs.Select(SynchronizationRunOverview.FromResponse).ToList();
    }

    private async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, uri);
        using var response = await SendAsync(request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
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

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to StayHub API.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("Synchronization did not respond in time.");
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            throw new UnauthorizedAccessException();
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            response.Dispose();
            throw new ApiException("Your role does not allow reservation synchronization.");
        }

        if (response.IsSuccessStatusCode)
            return response;

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        response.Dispose();
        throw new ApiException(string.IsNullOrWhiteSpace(message)
            ? "Synchronization failed."
            : message.Trim('"'));
    }
}
