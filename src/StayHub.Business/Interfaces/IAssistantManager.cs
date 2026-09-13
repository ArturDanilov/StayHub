using StayHub.Contracts.Assistant;

namespace StayHub.Business.Interfaces;

public interface IAssistantManager
{
    Task<AssistantChatResponse> ChatAsync(
        AssistantChatRequest request,
        CancellationToken cancellationToken = default);
}
