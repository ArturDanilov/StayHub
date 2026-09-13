namespace StayHub.Api.Assistant;

public static class AiAssistantProviders
{
    public const string Ollama = "Ollama";
    public const string AzureFoundry = "AzureFoundry";

    public static bool IsSupported(string? provider) =>
        string.Equals(provider, Ollama, StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider, AzureFoundry, StringComparison.OrdinalIgnoreCase);
}
