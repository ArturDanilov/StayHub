using StayHub.Domain.Models;

namespace StayHub.Business.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByNormalizedUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
