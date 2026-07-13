using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseHub.Api.Constants;
using PulseHub.Business.Interfaces;
using PulseHub.Contracts.Authentication;

namespace PulseHub.Api.Controllers;

[Authorize]
[ApiController]
[Route(ApiRoutes.Authentication.Base)]
public sealed class AuthController(
    IJwtTokenService jwtTokenService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost(ApiRoutes.Authentication.Login)]
    public ActionResult<LoginResponse> Login(
        LoginRequest request)
    {
        const string username = "admin";
        const string password = "admin";

        if (request.Username != username ||
            request.Password != password)
        {
            return Unauthorized();
        }

        var response = jwtTokenService.CreateToken(
            userId: 1,
            email: "admin@pulsehub.local",
            role: "Admin");

        return Ok(response);
    }
}
