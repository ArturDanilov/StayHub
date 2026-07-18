using StayHub.Contracts.Authentication;

namespace StayHub.Business.Interfaces;

public interface IJwtTokenService
{
    LoginResponse CreateToken(
        int userId,
        string email,
        string role);
}
