namespace StayHub.Contracts.Guests;

public sealed record GuestResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime CreatedAtUtc);
