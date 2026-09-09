using StayHub.Contracts.Reservations;

namespace StayHub.Mobile.Models;

public sealed record ReservationOverview(
    int Id,
    string ExternalId,
    int SourceId,
    string SourceName,
    int GuestId,
    string GuestName,
    string GuestEmail,
    string? GuestPhone,
    int PropertyId,
    string PropertyName,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status,
    DateTime CreatedAtUtc)
{
    public string StatusLabel => Status switch
    {
        ReservationStatusContract.CheckedIn => "Checked in",
        ReservationStatusContract.CheckedOut => "Checked out",
        _ => Status.ToString()
    };

    public string StatusColor => Status switch
    {
        ReservationStatusContract.Confirmed => "#126E82",
        ReservationStatusContract.CheckedIn => "#356859",
        ReservationStatusContract.CheckedOut => "#64748B",
        ReservationStatusContract.Cancelled => "#B3261E",
        _ => "#64748B"
    };
}
