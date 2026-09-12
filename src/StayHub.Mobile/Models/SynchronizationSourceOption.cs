namespace StayHub.Mobile.Models;

public sealed record SynchronizationSourceOption(int Id, string Name)
{
    public override string ToString() => Name;
}
