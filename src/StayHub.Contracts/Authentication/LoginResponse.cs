namespace StayHub.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Role);
