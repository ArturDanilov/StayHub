namespace StayHub.Contracts.Guests;

public sealed record CreateGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);
