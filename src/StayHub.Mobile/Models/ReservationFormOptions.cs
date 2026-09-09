namespace StayHub.Mobile.Models;

public sealed record PropertyOption(int Id, string Name)
{
    public override string ToString() => Name;
}

public sealed record GuestOption(int Id, string Name, string Email)
{
    public override string ToString() => $"{Name} · {Email}";
}

public sealed record SourceOption(int Id, string Name)
{
    public override string ToString() => Name;
}

public sealed record ReservationFormOptions(
    IReadOnlyList<PropertyOption> Properties,
    IReadOnlyList<GuestOption> Guests,
    IReadOnlyList<SourceOption> Sources);
