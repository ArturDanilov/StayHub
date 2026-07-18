namespace StayHub.Contracts.Sources;

public sealed record CreateSourceRequest(
    string Name,
    string SourceType,
    string? Url,
    bool IsEnabled);
