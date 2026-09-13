namespace StayHub.Api.Assistant;

public sealed class AiAssistantOptions
{
    public const string SectionName = "AiAssistant";
    public bool Enabled { get; init; }
    public string BaseAddress { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "qwen3:4b";
    public int TimeoutSeconds { get; init; } = 120;
}
