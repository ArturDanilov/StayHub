using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface IUsersService
{
    Task<IReadOnlyList<UserOverview>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(string username, string email, string password, string role, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(int id, string role, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(int id, string password, CancellationToken cancellationToken = default);
}
