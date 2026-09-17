using StayHub.Domain.Models;

namespace StayHub.Business.Models;

public sealed record DashboardSynchronizationAlert(
    int Id,
    int SourceId,
    string SourceName,
    SynchronizationStatus Status,
    DateTime StartedAtUtc,
    int ConflictCount,
    int FailedCount,
    string? ErrorMessage);
