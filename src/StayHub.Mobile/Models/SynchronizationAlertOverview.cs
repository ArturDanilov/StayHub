namespace StayHub.Mobile.Models;

public sealed record SynchronizationAlertOverview(
    int Id,
    string SourceName,
    string Status,
    DateTime StartedAtUtc,
    int ConflictCount,
    int FailedCount,
    string? ErrorMessage)
{
    public string Summary => $"{ConflictCount} conflicts · {FailedCount} failures";
}
