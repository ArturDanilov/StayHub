using Microsoft.EntityFrameworkCore;
using StayHub.Dal;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Api.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<StayHubDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Properties.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        #region Properties

        var properties = new List<Property>
        {
            new()
            {
                Name = "StayHub Munich Central",
                City = "Munich",
                CountryCode = "DE",
                CreatedAtUtc = now
            },
            new()
            {
                Name = "StayHub Berlin Mitte",
                City = "Berlin",
                CountryCode = "DE",
                CreatedAtUtc = now
            },
            new()
            {
                Name = "StayHub Lake Resort",
                City = "Rottach-Egern",
                CountryCode = "DE",
                CreatedAtUtc = now
            }
        };

        db.Properties.AddRange(properties);

        #endregion

        #region Guests

        var guests = new List<Guest>
        {
            new()
            {
                FirstName = "Anna",
                LastName = "Müller",
                Email = "anna.mueller@example.com",
                Phone = "+49 151 11111111",
                CreatedAtUtc = now
            },
            new()
            {
                FirstName = "Lukas",
                LastName = "Schneider",
                Email = "lukas.schneider@example.com",
                Phone = "+49 151 22222222",
                CreatedAtUtc = now
            },
            new()
            {
                FirstName = "Sophie",
                LastName = "Weber",
                Email = "sophie.weber@example.com",
                Phone = "+49 151 33333333",
                CreatedAtUtc = now
            },
            new()
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@example.com",
                Phone = "+1 555 123456",
                CreatedAtUtc = now
            },
            new()
            {
                FirstName = "Emily",
                LastName = "Johnson",
                Email = "emily.johnson@example.com",
                Phone = "+1 555 987654",
                CreatedAtUtc = now
            }
        };

        db.Guests.AddRange(guests);

        #endregion

        #region Sources

        var sources = new List<Source>
        {
            new()
            {
                Name = "Booking.com",
                SourceType = "OTA",
                Url = "https://booking.com",
                IsEnabled = true,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Apaleo",
                SourceType = "PMS",
                Url = "https://apaleo.com",
                IsEnabled = true,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Manual Import",
                SourceType = "Manual",
                IsEnabled = true,
                CreatedAtUtc = now
            }
        };

        db.Sources.AddRange(sources);

        await db.SaveChangesAsync(cancellationToken);

        #endregion

        #region Reservations

        var reservations = new List<Reservation>
        {
            new()
            {
                ExternalId = Guid.NewGuid().ToString("N"),
                GuestId = guests[0].Id,
                PropertyId = properties[0].Id,
                ArrivalDate = today.AddDays(2),
                DepartureDate = today.AddDays(5),
                Status = ReservationStatus.Confirmed,
                CreatedAtUtc = now
            },
            new()
            {
                ExternalId = Guid.NewGuid().ToString("N"),
                GuestId = guests[1].Id,
                PropertyId = properties[0].Id,
                ArrivalDate = today,
                DepartureDate = today.AddDays(4),
                Status = ReservationStatus.CheckedIn,
                CreatedAtUtc = now
            },
            new()
            {
                ExternalId = Guid.NewGuid().ToString("N"),
                GuestId = guests[2].Id,
                PropertyId = properties[1].Id,
                ArrivalDate = today.AddDays(-7),
                DepartureDate = today.AddDays(-3),
                Status = ReservationStatus.CheckedOut,
                CreatedAtUtc = now
            },
            new()
            {
                ExternalId = Guid.NewGuid().ToString("N"),
                GuestId = guests[3].Id,
                PropertyId = properties[2].Id,
                ArrivalDate = today.AddDays(10),
                DepartureDate = today.AddDays(14),
                Status = ReservationStatus.Confirmed,
                CreatedAtUtc = now
            },
            new()
            {
                ExternalId = Guid.NewGuid().ToString("N"),
                GuestId = guests[4].Id,
                PropertyId = properties[1].Id,
                ArrivalDate = today.AddDays(20),
                DepartureDate = today.AddDays(23),
                Status = ReservationStatus.Cancelled,
                CreatedAtUtc = now
            }
        };

        db.Reservations.AddRange(reservations);

        #endregion

        await db.SaveChangesAsync(cancellationToken);
    }
}
