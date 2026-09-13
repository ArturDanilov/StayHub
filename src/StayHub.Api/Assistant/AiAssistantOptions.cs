namespace StayHub.Api.Assistant;

public sealed class AiAssistantOptions
{
    public const string SectionName = "AiAssistant";
    public bool Enabled { get; init; }
    public string Provider { get; init; } = AiAssistantProviders.Ollama;
    public string BaseAddress { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "qwen3:4b";
    public string ApiKey { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 120;
    public int MaxOutputTokens { get; init; } = 300;
    public int DailyRequestLimit { get; init; } = 20;
}
