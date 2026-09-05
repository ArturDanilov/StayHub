using StayHub.Mobile.Models;

namespace StayHub.Mobile.Data;

public static class DemoData
{
    public static IReadOnlyList<ReservationOverview> Reservations { get; } =
    [
        new("Anna Müller", "StayHub Munich Central", new DateTime(2026, 9, 5), new DateTime(2026, 9, 8), "Confirmed", "#126E82"),
        new("John Smith", "StayHub Lake Resort", new DateTime(2026, 9, 7), new DateTime(2026, 9, 11), "Checked in", "#356859"),
        new("Sophie Weber", "StayHub Berlin Mitte", new DateTime(2026, 9, 12), new DateTime(2026, 9, 15), "Confirmed", "#126E82")
    ];
}
