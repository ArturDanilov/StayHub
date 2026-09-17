namespace StayHub.Contracts.Operations;

public sealed record OperationsDashboardResponse(
    DateOnly BusinessDate,
    DashboardSection<DashboardBookingResponse> Arrivals,
    DashboardSection<DashboardBookingResponse> Departures,
    DashboardSection<DashboardBookingResponse> InHouse,
    DashboardSection<DashboardBookingResponse> UpcomingArrivals,
    IReadOnlyList<SynchronizationAlertResponse> SynchronizationAlerts);
