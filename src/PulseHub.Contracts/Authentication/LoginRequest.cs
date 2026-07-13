namespace PulseHub.Contracts.Authentication;

public sealed record LoginRequest(
    string Username,
    string Password);
