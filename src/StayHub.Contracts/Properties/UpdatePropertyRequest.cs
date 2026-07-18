namespace StayHub.Contracts.Properties;

public sealed record UpdatePropertyRequest(
    string Name,
    string City,
    string CountryCode);
