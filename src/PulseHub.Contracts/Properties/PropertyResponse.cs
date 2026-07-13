namespace PulseHub.Contracts.Properties;

public sealed record PropertyResponse(
    int Id,
    string Name,
    string City,
    string CountryCode,
    DateTime CreatedAtUtc);
