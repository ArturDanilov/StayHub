using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayHub.Contracts.Operations;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public sealed class OperationsDashboardService(
    HttpClient httpClient,
    IAuthService authService)
    : IOperationsDashboardService
{
    public async Task<OperationsDashboardOverview> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var token = await authService.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException();

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/operations-dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            if (!response.IsSuccessStatusCode)
                throw new ApiException("Could not load today's operations.");

            var dashboard = await response.Content.ReadFromJsonAsync<OperationsDashboardResponse>(
                                cancellationToken: cancellationToken)
                            ?? throw new ApiException("The dashboard response was empty.");

            return Map(dashboard);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            || exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("Cannot connect to StayHub API. Start the API and try again.");
        }
    }

    private static OperationsDashboardOverview Map(OperationsDashboardResponse response)
    {
        return new OperationsDashboardOverview(
            response.BusinessDate,
            Map(response.Arrivals),
            Map(response.Departures),
            Map(response.InHouse),
            Map(response.UpcomingArrivals),
            response.SynchronizationAlerts
                .Select(x => new SynchronizationAlertOverview(
                    x.Id,
                    x.SourceName,
                    x.Status,
                    x.StartedAtUtc,
                    x.ConflictCount,
                    x.FailedCount,
                    x.ErrorMessage))
                .ToList());
    }

    private static DashboardSectionOverview Map(
        DashboardSection<DashboardBookingResponse> section)
    {
        return new DashboardSectionOverview(
            section.TotalCount,
            section.Items
                .Select(x => new DashboardBookingOverview(
                    x.Id,
                    x.ArrivalDate,
                    x.DepartureDate,
                    x.Status,
                    x.PropertyName,
                    $"{x.GuestFirstName} {x.GuestLastName}"))
                .ToList());
    }
}
