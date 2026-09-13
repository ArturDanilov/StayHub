using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StayHub.Api.Common;
using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Assistant;

namespace StayHub.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Assistant.Base)]
[Authorize(Policy = AuthorizationPolicies.ReadAccess)]
[EnableRateLimiting("assistant")]
public sealed class AssistantController(IAssistantManager assistantManager) : ControllerBase
{
    [HttpPost(ApiRoutes.Assistant.Chat)]
    public async Task<ActionResult<AssistantChatResponse>> Chat(
        AssistantChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await assistantManager.ChatAsync(request, cancellationToken));
        }
        catch (AssistantUnavailableException exception)
        {
            return Problem(
                title: "AI assistant is unavailable",
                detail: exception.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
