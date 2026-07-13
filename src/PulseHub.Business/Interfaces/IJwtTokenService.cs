using PulseHub.Contracts.Authentication;

namespace PulseHub.Business.Interfaces;

public interface IJwtTokenService
{
    LoginResponse CreateToken(
        int userId,
        string email,
        string role);
}
