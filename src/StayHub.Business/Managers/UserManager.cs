using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Users;
using StayHub.Domain.Models;

namespace StayHub.Business.Managers;

public sealed class UserManager(
    IUserRepository userRepository,
    IPasswordService passwordService) : IUserManager
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(ToResponse).ToList();
    }

    public async Task<OperationResult<UserResponse, UserError>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var normalizedUsername = username.ToUpperInvariant();
        var email = request.Email.Trim().ToLowerInvariant();

        if (!UserRoles.IsSupported(request.Role))
            return OperationResult<UserResponse, UserError>.Failure(UserError.UnsupportedRole);

        if (await userRepository.UsernameExistsAsync(normalizedUsername, cancellationToken))
            return OperationResult<UserResponse, UserError>.Failure(UserError.UsernameAlreadyExists);

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
            return OperationResult<UserResponse, UserError>.Failure(UserError.EmailAlreadyExists);

        var user = new User
        {
            Username = username,
            NormalizedUsername = normalizedUsername,
            Email = email,
            PasswordHash = string.Empty,
            Role = request.Role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = passwordService.Hash(user, request.Password);

        await userRepository.AddAsync(user, cancellationToken);
        return OperationResult<UserResponse, UserError>.Success(ToResponse(user));
    }

    public async Task<UserError> UpdateRoleAsync(
        int id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!UserRoles.IsSupported(request.Role))
            return UserError.UnsupportedRole;

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return UserError.UserNotFound;

        if (user.IsActive
            && user.Role == UserRoles.Admin
            && request.Role != UserRoles.Admin
            && await userRepository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return UserError.LastActiveAdmin;
        }

        user.Role = request.Role;
        await userRepository.SaveChangesAsync(cancellationToken);
        return UserError.None;
    }

    public async Task<UserError> UpdateStatusAsync(
        int id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return UserError.UserNotFound;

        if (!request.IsActive
            && user.IsActive
            && user.Role == UserRoles.Admin
            && await userRepository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return UserError.LastActiveAdmin;
        }

        user.IsActive = request.IsActive;
        await userRepository.SaveChangesAsync(cancellationToken);
        return UserError.None;
    }

    public async Task<UserError> ResetPasswordAsync(
        int id,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return UserError.UserNotFound;

        user.PasswordHash = passwordService.Hash(user, request.Password);
        await userRepository.SaveChangesAsync(cancellationToken);
        return UserError.None;
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc);
    }
}
