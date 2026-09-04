namespace StayHub.Mobile.Models;

public sealed record ReservationOverview(
    string GuestName,
    string PropertyName,
    DateTime ArrivalDate,
    DateTime DepartureDate,
    string Status,
    string StatusColor);
