using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Authentication;

public sealed record LoginRequest(
    [property: Required, StringLength(100, MinimumLength = 1)]
    string Username,
    [property: Required, StringLength(256, MinimumLength = 1)]
    string Password);
