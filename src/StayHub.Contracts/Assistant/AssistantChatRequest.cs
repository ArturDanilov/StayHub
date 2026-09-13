using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Assistant;

public sealed class AssistantChatRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<AssistantConversationMessage> History { get; init; } = [];
}
