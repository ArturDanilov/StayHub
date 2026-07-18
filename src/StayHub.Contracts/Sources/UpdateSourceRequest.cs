namespace StayHub.Contracts.Sources;

public sealed record UpdateSourceRequest(
    string Name,
    string SourceType,
    string? Url,
    bool IsEnabled);
