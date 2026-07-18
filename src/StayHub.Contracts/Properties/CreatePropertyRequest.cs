namespace StayHub.Contracts.Properties;

public sealed record CreatePropertyRequest(
    string Name,
    string City,
    string CountryCode);
