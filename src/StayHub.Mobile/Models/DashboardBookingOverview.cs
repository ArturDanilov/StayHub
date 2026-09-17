using StayHub.Contracts.Reservations;

namespace StayHub.Mobile.Models;

public sealed record DashboardBookingOverview(
    int Id,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status,
    string PropertyName,
    string GuestName)
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
