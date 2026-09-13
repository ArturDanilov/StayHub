using StayHub.Contracts.Assistant;

namespace StayHub.Mobile.Services;

public interface IAssistantService
{
    Task<AssistantChatResponse> SendAsync(
        AssistantChatRequest request,
        CancellationToken cancellationToken = default);
}
