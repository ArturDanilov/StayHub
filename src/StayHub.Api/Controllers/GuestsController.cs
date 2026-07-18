using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Guests;

namespace StayHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Guests.Base)]
public sealed class GuestsController(IGuestManager guestManager)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<IReadOnlyList<GuestResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var guests = await guestManager.GetAllAsync(cancellationToken);

        return Ok(guests);
    }

    [HttpGet(ApiRoutes.Guests.ById)]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<GuestResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var guest = await guestManager.GetByIdAsync(
            id,
            cancellationToken);

        return guest is null
            ? NotFound()
            : Ok(guest);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<ActionResult<GuestResponse>> Create(
        CreateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var result = await guestManager.CreateAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess)
            return MapError(result.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut(ApiRoutes.Guests.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<IActionResult> Update(
        int id,
        UpdateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var error = await guestManager.UpdateAsync(
            id,
            request,
            cancellationToken);

        return error == GuestError.None
            ? NoContent()
            : MapError(error);
    }

    [HttpDelete(ApiRoutes.Guests.ById)]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var error = await guestManager.DeleteAsync(
            id,
            cancellationToken);

        return error == GuestError.None
            ? NoContent()
            : MapError(error);
    }

    private ActionResult MapError(GuestError error)
    {
        return error switch
        {
            GuestError.GuestNotFound =>
                NotFound("Guest not found."),

            GuestError.EmailAlreadyExists =>
                Conflict("A guest with this email already exists."),

            GuestError.GuestHasReservations =>
                Conflict("Guest cannot be deleted because reservations exist."),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.")
        };
    }
}
