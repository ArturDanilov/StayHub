using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Authentication;

public sealed record LoginRequest(
    [Required, StringLength(100, MinimumLength = 1)]
    string Username,
    [Required, StringLength(256, MinimumLength = 1)]
    string Password);
