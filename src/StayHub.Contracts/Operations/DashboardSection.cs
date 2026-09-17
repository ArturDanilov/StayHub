namespace StayHub.Contracts.Operations;

public sealed record DashboardSection<T>(
    int TotalCount,
    IReadOnlyList<T> Items);
