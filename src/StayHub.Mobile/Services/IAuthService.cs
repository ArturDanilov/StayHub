namespace StayHub.Mobile.Services;

public interface IAuthService
{
    Task<bool> HasValidSessionAsync();
    Task<string?> GetAccessTokenAsync();
    Task LoginAsync(string username, string password, string role, CancellationToken cancellationToken = default);
    Task LogoutAsync();
}
