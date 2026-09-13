using StayHub.Contracts.Users;

namespace StayHub.Api.Common;

public static class AppRoles
{
    public const string Admin = UserRoles.Admin;
    public const string Receptionist = UserRoles.Receptionist;
    public const string Viewer = UserRoles.Viewer;
    public const string AdminOrReceptionist = $"{Admin},{Receptionist}";
    public const string All = $"{Admin},{Receptionist},{Viewer}";
}
