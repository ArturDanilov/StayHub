using StayHub.Contracts.Reservations;
using StayHub.Contracts.Synchronization;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var reservations = new List<ExternalReservationResponse>
{
    new(
        "MOCK-1001",
        "StayHub Lake Resort",
        "Toni",
        "Danilov",
        "toni.danilov@example.com",
        "+49 151 00000003",
        new DateOnly(2026, 7, 25),
        new DateOnly(2026, 7, 30),
        ReservationStatusContract.CheckedIn),
    new(
        "MOCK-1002",
        "StayHub Berlin Mitte",
        "Stas",
        "Starenko",
        "stas.starenko@example.com",
        "+49 151 00000009",
        new DateOnly(2026, 8, 4),
        new DateOnly(2026, 8, 7),
        ReservationStatusContract.Cancelled),
    new(
        "MOCK-2001",
        "Albrecht-Thaer-Straße 2",
        "Nik",
        "Scherbakov",
        "nik.scherbakov@example.com",
        "+49 151 00000007",
        new DateOnly(2026, 10, 18),
        new DateOnly(2026, 10, 22),
        ReservationStatusContract.Confirmed)
};

app.MapGet("/api/reservations", () => Results.Ok(reservations)).WithName("GetReservations");
app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));

app.Run();
