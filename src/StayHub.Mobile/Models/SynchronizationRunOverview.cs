using StayHub.Contracts.Synchronization;

namespace StayHub.Mobile.Models;

public sealed record SynchronizationRunOverview(
    int Id,
    string SourceName,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    int CreatedCount,
    int UpdatedCount,
    int UnchangedCount,
    int ConflictCount,
    int FailedCount,
    string? ErrorMessage)
{
    public string StartedLabel => StartedAtUtc.ToLocalTime().ToString("dd MMM yyyy, HH:mm");
    public string Summary => $"Created {CreatedCount}  •  Updated {UpdatedCount}  •  Unchanged {UnchangedCount}";
    public string Details => $"Conflicts {ConflictCount}  •  Failed {FailedCount}";
    public string StatusColor => Status switch
    {
        "Completed" => "#356859",
        "CompletedWithErrors" => "#C77800",
        "Failed" => "#B3261E",
        _ => "#19788C"
    };

    public static SynchronizationRunOverview FromResponse(SynchronizationRunResponse response) => new(
        response.Id,
        response.SourceName,
        response.Status,
        response.StartedAtUtc,
        response.CompletedAtUtc,
        response.CreatedCount,
        response.UpdatedCount,
        response.UnchangedCount,
        response.ConflictCount,
        response.FailedCount,
        response.ErrorMessage);
}
