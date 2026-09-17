using Microsoft.Extensions.Options;
using StayHub.Business.Interfaces;
using StayHub.Business.Models;
using StayHub.Business.Results;

namespace StayHub.Api.Assistant;

public sealed class OllamaAssistantClient(HttpClient httpClient, IOptions<AiAssistantOptions> options)
    : IAssistantClient
{
    public async Task<string> CompleteAsync(
        IReadOnlyList<AssistantPromptMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
            throw new AssistantUnavailableException("The local Ollama assistant is disabled.");

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/chat",
                new OllamaChatRequest(options.Value.Model, messages, false, false),
                cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(result?.Message?.Content))
                throw new AssistantUnavailableException("The local Ollama model returned an empty response.");

            return result.Message.Content.Trim();
        }
        catch (AssistantUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AssistantUnavailableException(
                "The local Ollama model did not respond in time. Make sure Ollama is running and try again.");
        }
        catch (HttpRequestException exception)
        {
            throw new AssistantUnavailableException(
                "Cannot connect to the local Ollama model. Start Ollama and try again.",
                exception);
        }
    }

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyList<AssistantPromptMessage> Messages,
        bool Stream,
        bool Think);

    private sealed record OllamaChatResponse(OllamaMessage? Message);

    private sealed record OllamaMessage(string Content);
}
