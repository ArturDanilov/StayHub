using StayHub.Business.Interfaces;
using StayHub.Contracts.Authentication;

namespace StayHub.Business.Managers;

public sealed class AuthenticationManager(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService)
    : IAuthenticationManager
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToUpperInvariant();
        var user = await userRepository.GetByNormalizedUsernameAsync(
            normalizedUsername,
            cancellationToken);

        if (user is null || !user.IsActive)
            return null;

        var verification = passwordService.Verify(
            user,
            user.PasswordHash,
            request.Password);

        if (verification == PasswordVerification.Failed)
            return null;

        if (verification == PasswordVerification.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordService.Hash(user, request.Password);
            await userRepository.SaveChangesAsync(cancellationToken);
        }

        return jwtTokenService.CreateToken(
            user.Id,
            user.Email,
            user.Role);
    }
}
