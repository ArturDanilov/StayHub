using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseHub.Api.Common;
using PulseHub.Business.Interfaces;
using PulseHub.Business.Results;
using PulseHub.Contracts.Reservations;

namespace PulseHub.Api.Controllers;

[Authorize]
[ApiController]
[Route(ApiRoutes.Reservations.Base)]
public sealed class ReservationsController(
    IReservationManager reservationManager)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var reservations =
            await reservationManager.GetAllAsync(cancellationToken);

        return Ok(reservations);
    }

    [HttpGet(ApiRoutes.Reservations.ById)]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<ReservationResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var reservation =
            await reservationManager.GetByIdAsync(id, cancellationToken);

        return reservation is null
            ? NotFound()
            : Ok(reservation);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<ActionResult<ReservationResponse>> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await reservationManager.CreateAsync(
                request,
                cancellationToken);

        if (!result.IsSuccess)
            return MapError(result.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut(ApiRoutes.Reservations.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<IActionResult> Update(
        int id,
        UpdateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var error =
            await reservationManager.UpdateAsync(
                id,
                request,
                cancellationToken);

        return error == ReservationError.None
            ? NoContent()
            : MapError(error);
    }

    [HttpPatch(ApiRoutes.Reservations.Status)]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateReservationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var error =
            await reservationManager.UpdateStatusAsync(
                id,
                request,
                cancellationToken);

        return error == ReservationError.None
            ? NoContent()
            : MapError(error);
    }

    [HttpDelete(ApiRoutes.Reservations.ById)]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await reservationManager.DeleteAsync(
                id,
                cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }

    private ActionResult MapError(ReservationError error)
    {
        return error switch
        {
            ReservationError.ReservationNotFound =>
                NotFound("Reservation not found."),

            ReservationError.GuestNotFound =>
                BadRequest("Guest not found."),
            
            ReservationError.PropertyNotFound =>
                BadRequest("Property not found."),

            ReservationError.ExternalIdAlreadyExists =>
                Conflict("A reservation with this external ID already exists."),

            ReservationError.InvalidDateRange =>
                BadRequest("Departure date must be later than arrival date."),

            ReservationError.InvalidStatusTransition =>
                BadRequest("The requested status transition is not allowed."),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.")
        };
    }
}
