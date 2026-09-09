using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Users;

public sealed record CreateUserRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Username,
    [Required, EmailAddress, StringLength(200)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    [Required] string Role);
