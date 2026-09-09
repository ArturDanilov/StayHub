using StayHub.Business.Results;
using StayHub.Contracts.Users;

namespace StayHub.Business.Interfaces;

public interface IUserManager
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<UserResponse, UserError>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserError> UpdateRoleAsync(int id, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<UserError> UpdateStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
    Task<UserError> ResetPasswordAsync(int id, ResetUserPasswordRequest request, CancellationToken cancellationToken = default);
}
