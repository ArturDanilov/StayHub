namespace StayHub.Mobile.Models;

public sealed record OperationsDashboardOverview(
    DateOnly BusinessDate,
    DashboardSectionOverview Arrivals,
    DashboardSectionOverview Departures,
    DashboardSectionOverview InHouse,
    DashboardSectionOverview UpcomingArrivals,
    IReadOnlyList<SynchronizationAlertOverview> SynchronizationAlerts);
