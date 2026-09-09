using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Contracts.Authentication;

namespace StayHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Authentication.Base)]
public sealed class AuthController(
    IAuthenticationManager authenticationManager)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost(ApiRoutes.Authentication.Login)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authenticationManager.LoginAsync(
            request,
            cancellationToken);

        return response is null
            ? Unauthorized("Invalid username or password.")
            : Ok(response);
    }
}
