using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseHub.Api.Common;
using PulseHub.Business.Interfaces;
using PulseHub.Contracts.Sources;

namespace PulseHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Sources.Base)]
public sealed class SourcesController(ISourceManager sourceManager)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<IReadOnlyList<SourceResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var sources = await sourceManager.GetAllAsync(cancellationToken);

        return Ok(sources);
    }

    [HttpGet(ApiRoutes.Sources.ById)]
    [Authorize(Policy = AuthorizationPolicies.ReadAccess)]
    public async Task<ActionResult<SourceResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var source = await sourceManager.GetByIdAsync(
            id,
            cancellationToken);

        return source is null
            ? NotFound()
            : Ok(source);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<ActionResult<SourceResponse>> Create(
        CreateSourceRequest request,
        CancellationToken cancellationToken)
    {
        var source = await sourceManager.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = source.Id },
            source);
    }

    [HttpPut(ApiRoutes.Sources.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageReservations)]
    public async Task<IActionResult> Update(
        int id,
        UpdateSourceRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sourceManager.UpdateAsync(
            id,
            request,
            cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpDelete(ApiRoutes.Sources.ById)]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await sourceManager.DeleteAsync(
            id,
            cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}
