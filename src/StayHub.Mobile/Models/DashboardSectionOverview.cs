namespace StayHub.Mobile.Models;

public sealed record DashboardSectionOverview(
    int TotalCount,
    IReadOnlyList<DashboardBookingOverview> Items);
