using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Users;

public sealed record ResetUserPasswordRequest(
    [Required, StringLength(128, MinimumLength = 8)] string Password);
