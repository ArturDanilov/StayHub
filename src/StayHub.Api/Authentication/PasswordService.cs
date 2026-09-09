using Microsoft.AspNetCore.Identity;
using StayHub.Business.Interfaces;
using StayHub.Domain.Models;

namespace StayHub.Api.Authentication;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public PasswordVerification Verify(User user, string passwordHash, string password)
    {
        return _hasher.VerifyHashedPassword(user, passwordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordVerification.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerification.SuccessRehashNeeded,
            _ => PasswordVerification.Failed
        };
    }
}
