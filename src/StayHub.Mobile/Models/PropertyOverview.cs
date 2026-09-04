namespace StayHub.Mobile.Models;

public sealed record PropertyOverview(
    string Name,
    string Location,
    int UpcomingReservations,
    string Occupancy);
