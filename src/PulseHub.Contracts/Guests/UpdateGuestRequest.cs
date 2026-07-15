namespace PulseHub.Contracts.Guests;

public sealed record UpdateGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);
