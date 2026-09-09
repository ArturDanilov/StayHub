using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayHub.Contracts.Users;
using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public sealed class UsersService(HttpClient httpClient, IAuthService authService) : IUsersService
{
    public async Task<IReadOnlyList<UserOverview>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Get, "api/users");
        using var response = await SendAsync(request, cancellationToken);
        var users = await response.Content.ReadFromJsonAsync<IReadOnlyList<UserResponse>>(
                        cancellationToken: cancellationToken) ?? [];
        return users.Select(Map).ToList();
    }

    public async Task CreateAsync(
        string username,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Post, "api/users");
        request.Content = JsonContent.Create(new CreateUserRequest(username, email, password, role));
        using var response = await SendAsync(request, cancellationToken);
    }

    public async Task UpdateRoleAsync(int id, string role, CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Patch, $"api/users/{id}/role");
        request.Content = JsonContent.Create(new UpdateUserRoleRequest(role));
        using var response = await SendAsync(request, cancellationToken);
    }

    public async Task UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Patch, $"api/users/{id}/status");
        request.Content = JsonContent.Create(new UpdateUserStatusRequest(isActive));
        using var response = await SendAsync(request, cancellationToken);
    }

    public async Task ResetPasswordAsync(int id, string password, CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Put, $"api/users/{id}/password");
        request.Content = JsonContent.Create(new ResetUserPasswordRequest(password));
        using var response = await SendAsync(request, cancellationToken);
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string uri)
    {
        var token = await authService.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException();

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to StayHub API. Start the API and try again.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("StayHub API did not respond in time.");
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            throw new UnauthorizedAccessException();
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            response.Dispose();
            throw new ApiException("Only administrators can manage users.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            response.Dispose();
            throw new ApiException(string.IsNullOrWhiteSpace(message)
                ? "The user operation failed."
                : message.Trim('"'));
        }

        return response;
    }

    private static UserOverview Map(UserResponse user)
    {
        return new UserOverview(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc);
    }
}
