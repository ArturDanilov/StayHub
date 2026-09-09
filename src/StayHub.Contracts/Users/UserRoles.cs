namespace StayHub.Contracts.Users;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Receptionist = "Receptionist";
    public const string Viewer = "Viewer";

    public static bool IsSupported(string role)
    {
        return role is Admin or Receptionist or Viewer;
    }
}
