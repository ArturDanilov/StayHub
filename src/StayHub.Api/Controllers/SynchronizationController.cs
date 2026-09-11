using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Synchronization;

namespace StayHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Synchronization.Base)]
[Authorize(Policy = AuthorizationPolicies.ManageReservations)]
public sealed class SynchronizationController(ISynchronizationManager synchronizationManager)
    : ControllerBase
{
    [HttpPost(ApiRoutes.Synchronization.Source)]
    public async Task<ActionResult<SynchronizationRunResponse>> Synchronize(
        int sourceId,
        CancellationToken cancellationToken)
    {
        var result = await synchronizationManager.SynchronizeAsync(sourceId, cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error switch
        {
            SynchronizationError.SourceNotFound => NotFound("Source not found."),
            SynchronizationError.SourceDisabled => BadRequest("Source is disabled."),
            SynchronizationError.SourceUrlMissing => BadRequest("Source URL is not configured."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet(ApiRoutes.Synchronization.Runs)]
    public async Task<ActionResult<IReadOnlyList<SynchronizationRunResponse>>> GetRecentRuns(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await synchronizationManager.GetRecentRunsAsync(take, cancellationToken));
    }
}
