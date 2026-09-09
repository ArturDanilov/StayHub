using StayHub.Contracts.Authentication;

namespace StayHub.Business.Interfaces;

public interface IAuthenticationManager
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
