namespace StayHub.Mobile.Services;

public interface IAuthService
{
    Task<bool> HasValidSessionAsync();
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRoleAsync();
    Task LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync();
}
