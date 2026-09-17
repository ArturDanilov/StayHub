using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Contracts.Operations;

namespace StayHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.OperationsDashboard.Base)]
[Authorize(Policy = AuthorizationPolicies.ReadAccess)]
public sealed class OperationsDashboardController(
    IOperationsDashboardManager dashboardManager)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OperationsDashboardResponse>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(await dashboardManager.GetAsync(cancellationToken));
    }
}
