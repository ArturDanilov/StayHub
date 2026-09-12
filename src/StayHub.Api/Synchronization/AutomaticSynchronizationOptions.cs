namespace StayHub.Api.Synchronization;

public sealed class AutomaticSynchronizationOptions
{
    public const string SectionName = "AutomaticSynchronization";

    public bool Enabled { get; init; }

    public int IntervalMinutes { get; init; } = 360;

    public int InitialDelaySeconds { get; init; } = 30;

    public string[] SourceNames { get; init; } = [];
}
