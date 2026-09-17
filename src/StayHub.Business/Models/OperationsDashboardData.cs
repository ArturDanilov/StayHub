namespace StayHub.Business.Models;

public sealed record OperationsDashboardData(
    DashboardSectionResult<DashboardBooking> Arrivals,
    DashboardSectionResult<DashboardBooking> Departures,
    DashboardSectionResult<DashboardBooking> InHouse,
    DashboardSectionResult<DashboardBooking> UpcomingArrivals,
    IReadOnlyList<DashboardSynchronizationAlert> SynchronizationAlerts);
