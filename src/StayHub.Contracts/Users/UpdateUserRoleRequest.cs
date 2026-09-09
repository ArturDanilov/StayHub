using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Users;

public sealed record UpdateUserRoleRequest([Required] string Role);
