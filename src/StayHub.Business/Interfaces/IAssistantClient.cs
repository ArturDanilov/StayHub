using StayHub.Business.Models;

namespace StayHub.Business.Interfaces;

public interface IAssistantClient
{
    Task<string> CompleteAsync(
        IReadOnlyList<AssistantPromptMessage> messages,
        CancellationToken cancellationToken = default);
}
