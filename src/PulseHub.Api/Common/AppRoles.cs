namespace PulseHub.Api.Common;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Receptionist = "Receptionist";
    public const string Viewer = "Viewer";

    public const string AdminOrReceptionist =
        $"{Admin},{Receptionist}";

    public const string All =
        $"{Admin},{Receptionist},{Viewer}";
}
