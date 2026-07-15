namespace PulseHub.Contracts.Sources;

public sealed record SourceResponse(
    int Id,
    string Name,
    string SourceType,
    string? Url,
    bool IsEnabled,
    DateTime CreatedAtUtc);
