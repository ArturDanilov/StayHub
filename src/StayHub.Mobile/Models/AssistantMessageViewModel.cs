namespace StayHub.Mobile.Models;

public sealed record AssistantMessageViewModel(string Role, string Content)
{
    public bool IsUser => Role.Equals("user", StringComparison.OrdinalIgnoreCase);
}
