using System.Net;
using System.Net.Http.Json;
using StayHub.Contracts.Authentication;

namespace StayHub.Mobile.Services;

public sealed class AuthService(HttpClient httpClient) : IAuthService
{
    private const string AccessTokenKey = "stayhub_access_token";
    private const string ExpiresAtKey = "stayhub_access_token_expires_at";

    public async Task LoginAsync(
        string username,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/auth/login",
                new LoginRequest(username, password, role),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new ApiException("Incorrect username or password.");

            if (!response.IsSuccessStatusCode)
                throw new ApiException("StayHub API rejected the login request.");

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
                            cancellationToken: cancellationToken)
                        ?? throw new ApiException("StayHub API returned an empty response.");

            await SecureStorage.Default.SetAsync(AccessTokenKey, login.AccessToken);
            await SecureStorage.Default.SetAsync(ExpiresAtKey, login.ExpiresAtUtc.ToString("O"));
        }
        catch (ApiException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to StayHub API. Start the API and try again.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("StayHub API did not respond in time.");
        }
    }

    public async Task<bool> HasValidSessionAsync()
    {
        var token = await GetAccessTokenAsync();
        var expiresAtValue = await SecureStorage.Default.GetAsync(ExpiresAtKey);

        return !string.IsNullOrWhiteSpace(token)
               && DateTime.TryParse(expiresAtValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiresAt)
               && expiresAt > DateTime.UtcNow;
    }

    public Task<string?> GetAccessTokenAsync()
    {
        return SecureStorage.Default.GetAsync(AccessTokenKey);
    }

    public Task LogoutAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(ExpiresAtKey);
        return Task.CompletedTask;
    }
}
