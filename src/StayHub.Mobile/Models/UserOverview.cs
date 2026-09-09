namespace StayHub.Mobile.Models;

public sealed record UserOverview(
    int Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc)
{
    public string StatusLabel => IsActive ? "Active" : "Blocked";
    public string StatusColor => IsActive ? "#356859" : "#B3261E";
}
