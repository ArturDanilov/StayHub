using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Contracts.Authentication;

namespace StayHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Authentication.Base)]
public sealed class AuthController(
    IJwtTokenService jwtTokenService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost(ApiRoutes.Authentication.Login)]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        const string testUsername = "admin";
        const string testPassword = "admin";

        if (request.Username != testUsername ||
            request.Password != testPassword)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (!IsSupportedRole(request.Role))
            return BadRequest("Unsupported role.");

        var response = jwtTokenService.CreateToken(
            userId: 1,
            email: "admin@pulsehub.local",
            role: request.Role);

        return Ok(response);
    }

    private static bool IsSupportedRole(string role)
    {
        return role is
            AppRoles.Admin or
            AppRoles.Receptionist or
            AppRoles.Viewer;
    }
}
