using Microsoft.AspNetCore.Mvc;
using PulseHub.Api.Constants;
using PulseHub.Business.Interfaces;
using PulseHub.Contracts.Properties;

namespace PulseHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Properties.Base)]
public sealed class PropertiesController(IPropertyManager propertyManager)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertyResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var properties = await propertyManager.GetAllAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpGet(ApiRoutes.Properties.ById)]
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
