using StayHub.Domain.Models;

namespace StayHub.Business.Interfaces;

public interface IPasswordService
{
    string Hash(User user, string password);
    PasswordVerification Verify(User user, string passwordHash, string password);
}
