using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Api.Common;
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

        await SeedAdminAsync(
            scope.ServiceProvider,
            db,
            cancellationToken);

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>("SeedData:DemoDataEnabled"))
            return;

        if (await db.Properties.AnyAsync(cancellationToken))
        {
            await EnsureMockPmsSourceAsync(db, cancellationToken);
            await EnsureMockPmsPropertiesAsync(db, cancellationToken);
            return;
        }

        var createdAtUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        var referenceDate = new DateOnly(2026, 7, 15);

        #region Properties

        var properties = new List<Property>
        {
            new()
            {
                Name = "StayHub Munich Central",
                City = "Munich",
                CountryCode = "DE",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "StayHub Berlin Mitte",
                City = "Berlin",
                CountryCode = "DE",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "StayHub Lake Resort",
                City = "Rottach-Egern",
                CountryCode = "DE",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "Albrecht-Thaer-Straße 2",
                City = "Nürnberg",
                CountryCode = "DE",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "Planegger Straße 112",
                City = "München",
                CountryCode = "DE",
                CreatedAtUtc = createdAtUtc
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
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Lukas",
                LastName = "Schneider",
                Email = "lukas.schneider@example.com",
                Phone = "+49 151 22222222",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Sophie",
                LastName = "Weber",
                Email = "sophie.weber@example.com",
                Phone = "+49 151 33333333",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@example.com",
                Phone = "+1 555 123456",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Emily",
                LastName = "Johnson",
                Email = "emily.johnson@example.com",
                Phone = "+1 555 987654",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Artur",
                LastName = "Danilov",
                Email = "artur.danilov@example.com",
                Phone = "+49 151 00000001",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Iana",
                LastName = "Danilova",
                Email = "iana.danilova@example.com",
                Phone = "+49 151 00000002",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Toni",
                LastName = "Danilov",
                Email = "toni.danilov@example.com",
                Phone = "+49 151 00000003",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Alex",
                LastName = "Danilov",
                Email = "alex.danilov@example.com",
                Phone = "+49 151 00000004",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Maria",
                LastName = "Nedobor",
                Email = "maria.nedobor@example.com",
                Phone = "+49 151 00000005",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Timo",
                LastName = "Nedobor",
                Email = "timo.nedobor@example.com",
                Phone = "+49 151 00000006",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Nik",
                LastName = "Scherbakov",
                Email = "nik.scherbakov@example.com",
                Phone = "+49 151 00000007",
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                FirstName = "Julia",
                LastName = "Scherbakova",
                Email = "julia.scherbakova@example.com",
                Phone = "+49 151 00000008",
                CreatedAtUtc = createdAtUtc
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
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "Apaleo",
                SourceType = "PMS",
                Url = "https://apaleo.com",
                IsEnabled = true,
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "Manual Import",
                SourceType = "Manual",
                IsEnabled = true,
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                Name = "Mock PMS",
                SourceType = "PMS",
                Url = "http://localhost:5095",
                IsEnabled = true,
                CreatedAtUtc = createdAtUtc
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
                ExternalId = "BOOKING-1001",
                SourceId = sources[0].Id,
                GuestId = guests[0].Id,
                PropertyId = properties[0].Id,
                ArrivalDate = referenceDate.AddDays(2),
                DepartureDate = referenceDate.AddDays(5),
                Status = ReservationStatus.Confirmed,
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                ExternalId = "APALEO-1001",
                SourceId = sources[1].Id,
                GuestId = guests[1].Id,
                PropertyId = properties[0].Id,
                ArrivalDate = referenceDate,
                DepartureDate = referenceDate.AddDays(4),
                Status = ReservationStatus.CheckedIn,
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                ExternalId = "MANUAL-1001",
                SourceId = sources[2].Id,
                GuestId = guests[2].Id,
                PropertyId = properties[1].Id,
                ArrivalDate = referenceDate.AddDays(-7),
                DepartureDate = referenceDate.AddDays(-3),
                Status = ReservationStatus.CheckedOut,
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                ExternalId = "MOCK-1001",
                SourceId = sources[3].Id,
                GuestId = guests[3].Id,
                PropertyId = properties[2].Id,
                ArrivalDate = referenceDate.AddDays(10),
                DepartureDate = referenceDate.AddDays(14),
                Status = ReservationStatus.Confirmed,
                CreatedAtUtc = createdAtUtc
            },
            new()
            {
                ExternalId = "MOCK-1002",
                SourceId = sources[3].Id,
                GuestId = guests[4].Id,
                PropertyId = properties[1].Id,
                ArrivalDate = referenceDate.AddDays(20),
                DepartureDate = referenceDate.AddDays(23),
                Status = ReservationStatus.Cancelled,
                CreatedAtUtc = createdAtUtc
            }
        };

        db.Reservations.AddRange(reservations);

        #endregion

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureMockPmsSourceAsync(
        StayHubDbContext db,
        CancellationToken cancellationToken)
    {
        var mockPms = await db.Sources.FirstOrDefaultAsync(
            source => source.Name == "Mock PMS",
            cancellationToken);

        if (mockPms is null)
        {
            db.Sources.Add(new Source
            {
                Name = "Mock PMS",
                SourceType = "PMS",
                Url = "http://localhost:5095",
                IsEnabled = true,
                CreatedAtUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
            });
        }
        else
        {
            mockPms.SourceType = "PMS";
            mockPms.Url = "http://localhost:5095";
            mockPms.IsEnabled = true;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureMockPmsPropertiesAsync(
        StayHubDbContext db,
        CancellationToken cancellationToken)
    {
        var requiredProperties = new[]
        {
            new { Name = "Albrecht-Thaer-Straße 2", City = "Nürnberg" },
            new { Name = "Planegger Straße 112", City = "München" }
        };
        var existingNames = await db.Properties
            .Where(property => requiredProperties.Select(required => required.Name).Contains(property.Name))
            .Select(property => property.Name)
            .ToListAsync(cancellationToken);

        foreach (var property in requiredProperties.Where(property => !existingNames.Contains(property.Name)))
        {
            db.Properties.Add(new Property
            {
                Name = property.Name,
                City = property.City,
                CountryCode = "DE",
                CreatedAtUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAdminAsync(
        IServiceProvider serviceProvider,
        StayHubDbContext db,
        CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken))
            return;

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseSeeder));
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No admin user was created. Configure SeedAdmin:Password with user-secrets or an environment variable.");
            return;
        }

        if (password.Length < 8)
        {
            throw new InvalidOperationException(
                "SeedAdmin:Password must contain at least 8 characters.");
        }

        var username = configuration["SeedAdmin:Username"] ?? "admin";
        var email = configuration["SeedAdmin:Email"] ?? "admin@stayhub.local";
        var user = new User
        {
            Username = username,
            NormalizedUsername = username.ToUpperInvariant(),
            Email = email.Trim(),
            PasswordHash = string.Empty,
            Role = AppRoles.Admin,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc)
        };

        var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
        user.PasswordHash = passwordService.Hash(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
