namespace StayHub.Contracts.Users;

public sealed record UserResponse(
    int Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);
