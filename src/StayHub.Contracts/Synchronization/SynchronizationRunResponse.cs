namespace StayHub.Contracts.Synchronization;

public sealed record SynchronizationRunResponse(
    int Id,
    int SourceId,
    string SourceName,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    int CreatedCount,
    int UpdatedCount,
    int UnchangedCount,
    int ConflictCount,
    int FailedCount,
    string? ErrorMessage);
