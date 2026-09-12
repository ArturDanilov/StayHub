namespace StayHub.Domain.Models;

public class SynchronizationRun
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public Source Source { get; set; } = null!;
    public SynchronizationStatus Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int UnchangedCount { get; set; }
    public int ConflictCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
}
