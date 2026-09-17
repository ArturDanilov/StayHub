namespace StayHub.Business.Models;

public sealed record DashboardSectionResult<T>(
    int TotalCount,
    IReadOnlyList<T> Items);
