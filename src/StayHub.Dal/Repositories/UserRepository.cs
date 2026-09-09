using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Contracts.Users;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class UserRepository(StayHubDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByNormalizedUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.NormalizedUsername == normalizedUsername,
            cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(string normalizedUsername, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Users.CountAsync(user => user.IsActive && user.Role == UserRoles.Admin, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
