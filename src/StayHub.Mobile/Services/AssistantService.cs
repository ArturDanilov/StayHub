using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StayHub.Contracts.Assistant;

namespace StayHub.Mobile.Services;

public sealed class AssistantService(HttpClient httpClient, IAuthService authService) : IAssistantService
{
    public async Task<AssistantChatResponse> SendAsync(
        AssistantChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await authService.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException();

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/assistant/chat")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(message, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to StayHub Assistant.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("StayHub Assistant did not respond in time.");
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                throw new ApiException(await ReadUnavailableMessageAsync(response, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ApiException(string.IsNullOrWhiteSpace(error)
                    ? "StayHub Assistant request failed."
                    : error.Trim('"'));
            }

            return await response.Content.ReadFromJsonAsync<AssistantChatResponse>(
                       cancellationToken: cancellationToken)
                   ?? throw new ApiException("StayHub Assistant returned an empty response.");
        }
    }

    private static async Task<string> ReadUnavailableMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return problem.Detail;
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }

        return "StayHub Assistant is temporarily unavailable.";
    }

    private sealed class ApiProblemDetails
    {
        public string? Detail { get; init; }
    }
}
