namespace StayHub.Contracts.Operations;

public sealed record SynchronizationAlertResponse(
    int Id,
    int SourceId,
    string SourceName,
    string Status,
    DateTime StartedAtUtc,
    int ConflictCount,
    int FailedCount,
    string? ErrorMessage);
