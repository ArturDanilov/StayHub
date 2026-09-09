using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Users;

namespace StayHub.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route(ApiRoutes.Users.Base)]
public sealed class UsersController(IUserManager userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await userManager.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userManager.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? Created($"{ApiRoutes.Users.Base}/{result.Value!.Id}", result.Value)
            : MapError(result.Error);
    }

    [HttpPatch(ApiRoutes.Users.Role)]
    public async Task<IActionResult> UpdateRole(
        int id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        return MapResult(await userManager.UpdateRoleAsync(id, request, cancellationToken));
    }

    [HttpPatch(ApiRoutes.Users.Status)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        return MapResult(await userManager.UpdateStatusAsync(id, request, cancellationToken));
    }

    [HttpPut(ApiRoutes.Users.Password)]
    public async Task<IActionResult> ResetPassword(
        int id,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return MapResult(await userManager.ResetPasswordAsync(id, request, cancellationToken));
    }

    private IActionResult MapResult(UserError error)
    {
        return error == UserError.None ? NoContent() : MapError(error);
    }

    private ObjectResult MapError(UserError error)
    {
        return error switch
        {
            UserError.UserNotFound => NotFound("User not found."),
            UserError.UsernameAlreadyExists => Conflict("Username already exists."),
            UserError.EmailAlreadyExists => Conflict("Email already exists."),
            UserError.UnsupportedRole => BadRequest("Unsupported role."),
            UserError.LastActiveAdmin => Conflict("The last active admin cannot be disabled or demoted."),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
    }
}
