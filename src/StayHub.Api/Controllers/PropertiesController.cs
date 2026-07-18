using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Contracts.Properties;

namespace StayHub.Api.Controllers;

[Authorize]
[ApiController]
[Route(ApiRoutes.Properties.Base)]
public sealed class PropertiesController(IPropertyManager propertyManager)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<IReadOnlyList<PropertyResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var properties = await propertyManager.GetAllAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpGet(ApiRoutes.Properties.ById)]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<PropertyResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var property = await propertyManager.GetByIdAsync(
            id,
            cancellationToken);

        if (property is null)
            return NotFound();

        return Ok(property);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<ActionResult<PropertyResponse>> Create(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var property = await propertyManager.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = property.Id },
            property);
    }

    [HttpPut(ApiRoutes.Properties.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<IActionResult> Update(
        int id,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await propertyManager.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete(ApiRoutes.Properties.ById)]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await propertyManager.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
