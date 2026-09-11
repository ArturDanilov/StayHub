namespace StayHub.Domain.Models;

public enum SynchronizationStatus
{
    Running = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4
}
