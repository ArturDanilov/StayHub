using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StayHub.Business.Interfaces;
using StayHub.Business.Models;
using StayHub.Business.Results;

namespace StayHub.Api.Assistant;

public sealed class AzureFoundryAssistantClient(
    HttpClient httpClient,
    IOptions<AiAssistantOptions> options) : IAssistantClient
{
    public async Task<string> CompleteAsync(
        IReadOnlyList<AssistantPromptMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
            throw new AssistantUnavailableException("Azure AI assistant is disabled.");

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "chat/completions",
                new
                {
                    model = options.Value.Model,
                    messages,
                    max_completion_tokens = options.Value.MaxOutputTokens
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AzureChatResponse>(
                cancellationToken: cancellationToken);
            var content = result?.Choices.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
                throw new AssistantUnavailableException("Azure AI returned an empty response.");

            return content.Trim();
        }
        catch (AssistantUnavailableException) { throw; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AssistantUnavailableException("Azure AI did not respond in time. Try again later.");
        }
        catch (HttpRequestException exception)
        {
            throw new AssistantUnavailableException(
                "Azure AI is temporarily unavailable. Try again later.",
                exception);
        }
    }

    private sealed record AzureChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<AzureChoice> Choices);
    private sealed record AzureChoice(
        [property: JsonPropertyName("message")] AzureMessage Message);
    private sealed record AzureMessage(
        [property: JsonPropertyName("content")] string Content);
}
