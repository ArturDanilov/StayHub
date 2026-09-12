using StayHub.Business.Interfaces;
using StayHub.Business.Managers;
using StayHub.Business.Models;
using StayHub.Business.Results;
using StayHub.Contracts.Reservations;
using StayHub.Contracts.Synchronization;
using StayHub.Domain.Models;
using Xunit;

namespace StayHub.Business.Tests.Managers;

public sealed class SynchronizationManagerTests
{
    [Fact]
    public async Task SynchronizeAsync_FirstImport_CreatesReservations()
    {
        var context = new TestContext();
        context.ExternalClient.Reservations =
        [
            CreateExternalReservation("MOCK-1001"),
            CreateExternalReservation(
                "MOCK-1002",
                arrivalDate: new DateOnly(2026, 8, 4),
                departureDate: new DateOnly(2026, 8, 7))
        ];

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Completed", result.Value.Status);
        Assert.Equal(2, result.Value.CreatedCount);
        Assert.Equal(0, result.Value.UpdatedCount);
        Assert.Equal(0, result.Value.UnchangedCount);
        Assert.Equal(2, context.ReservationRepository.Reservations.Count);
    }

    [Fact]
    public async Task SynchronizeAsync_RepeatedImport_MarksReservationAsUnchanged()
    {
        var context = new TestContext();
        context.ExternalClient.Reservations = [CreateExternalReservation()];
        await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(0, result.Value.CreatedCount);
        Assert.Equal(0, result.Value.UpdatedCount);
        Assert.Equal(1, result.Value.UnchangedCount);
        Assert.Single(context.ReservationRepository.Reservations);
    }

    [Theory]
    [InlineData(UpdateKind.Guest)]
    [InlineData(UpdateKind.Dates)]
    [InlineData(UpdateKind.Status)]
    public async Task SynchronizeAsync_ChangedExternalReservation_UpdatesReservation(UpdateKind updateKind)
    {
        var context = new TestContext();
        var original = CreateExternalReservation();
        context.ExternalClient.Reservations = [original];
        await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        context.ExternalClient.Reservations =
        [
            updateKind switch
            {
                UpdateKind.Guest => original with
                {
                    GuestFirstName = "Toni",
                    GuestLastName = "Danilov",
                    GuestEmail = "toni.danilov@example.com"
                },
                UpdateKind.Dates => original with
                {
                    ArrivalDate = new DateOnly(2026, 9, 10),
                    DepartureDate = new DateOnly(2026, 9, 14)
                },
                UpdateKind.Status => original with { Status = ReservationStatusContract.Cancelled },
                _ => throw new ArgumentOutOfRangeException(nameof(updateKind), updateKind, null)
            }
        ];

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(0, result.Value.CreatedCount);
        Assert.Equal(1, result.Value.UpdatedCount);
        Assert.Equal(0, result.Value.UnchangedCount);

        var reservation = Assert.Single(context.ReservationRepository.Reservations);
        switch (updateKind)
        {
            case UpdateKind.Guest:
                Assert.Equal("Toni", reservation.Guest.FirstName);
                Assert.Equal("toni.danilov@example.com", reservation.Guest.Email);
                break;
            case UpdateKind.Dates:
                Assert.Equal(new DateOnly(2026, 9, 10), reservation.ArrivalDate);
                Assert.Equal(new DateOnly(2026, 9, 14), reservation.DepartureDate);
                break;
            case UpdateKind.Status:
                Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
                break;
        }
    }

    [Fact]
    public async Task SynchronizeAsync_OverlappingReservation_CountsConflict()
    {
        var context = new TestContext();
        context.AddExistingReservation(
            externalId: "EXISTING-1",
            arrivalDate: new DateOnly(2026, 7, 20),
            departureDate: new DateOnly(2026, 7, 27));
        context.ExternalClient.Reservations =
        [
            CreateExternalReservation(
                externalId: "MOCK-CONFLICT",
                arrivalDate: new DateOnly(2026, 7, 25),
                departureDate: new DateOnly(2026, 7, 29))
        ];

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.CreatedCount);
        Assert.Equal(1, result.Value.ConflictCount);
    }

    [Fact]
    public async Task SynchronizeAsync_UnknownProperty_RecordsFailedReservationAndContinues()
    {
        var context = new TestContext();
        context.ExternalClient.Reservations =
        [
            CreateExternalReservation(propertyName: "Unknown property"),
            CreateExternalReservation(
                externalId: "MOCK-VALID",
                arrivalDate: new DateOnly(2026, 8, 4),
                departureDate: new DateOnly(2026, 8, 7))
        ];

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("CompletedWithErrors", result.Value.Status);
        Assert.Equal(1, result.Value.CreatedCount);
        Assert.Equal(1, result.Value.FailedCount);
        Assert.Contains("Property 'Unknown property' was not found", result.Value.ErrorMessage);
    }

    [Fact]
    public async Task SynchronizeAsync_UnknownSource_ReturnsSourceNotFound()
    {
        var context = new TestContext();

        var result = await context.Manager.SynchronizeAsync(
            999,
            Xunit.TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(SynchronizationError.SourceNotFound, result.Error);
        Assert.Empty(context.RunRepository.Runs);
    }

    [Fact]
    public async Task SynchronizeAsync_WhenSourceIsAlreadyRunning_ReturnsAlreadyRunning()
    {
        var context = new TestContext();
        using var existingExecution = context.ExecutionGate.TryAcquire(context.Source.Id);

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.NotNull(existingExecution);
        Assert.False(result.IsSuccess);
        Assert.Equal(SynchronizationError.AlreadyRunning, result.Error);
        Assert.Empty(context.RunRepository.Runs);
        Assert.Equal(0, context.ExternalClient.RequestCount);
    }

    [Fact]
    public async Task SynchronizeAsync_ExternalPmsFailure_MarksRunAsFailed()
    {
        var context = new TestContext();
        context.ExternalClient.Exception = new HttpRequestException("Mock PMS is unavailable.");

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Failed", result.Value.Status);
        Assert.Equal("Mock PMS is unavailable.", result.Value.ErrorMessage);
        Assert.NotNull(result.Value.CompletedAtUtc);
    }

    [Fact]
    public async Task SynchronizeAsync_ExternalPmsTimeout_MarksRunAsFailed()
    {
        var context = new TestContext();
        context.ExternalClient.Exception = new TaskCanceledException("The external PMS request timed out.");

        var result = await context.Manager.SynchronizeAsync(
            context.Source.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Failed", result.Value.Status);
        Assert.Equal("The external PMS request timed out.", result.Value.ErrorMessage);
        Assert.NotNull(result.Value.CompletedAtUtc);
    }

    private static ExternalReservationResponse CreateExternalReservation(
        string externalId = "MOCK-1001",
        string propertyName = "StayHub Lake Resort",
        DateOnly? arrivalDate = null,
        DateOnly? departureDate = null) =>
        new(
            externalId,
            propertyName,
            "John",
            "Smith",
            "john.smith@example.com",
            "+49 151 00000001",
            arrivalDate ?? new DateOnly(2026, 7, 25),
            departureDate ?? new DateOnly(2026, 7, 29),
            ReservationStatusContract.Confirmed);

    public enum UpdateKind
    {
        Guest,
        Dates,
        Status
    }

    private sealed class TestContext
    {
        public TestContext()
        {
            Source = new Source
            {
                Id = 1,
                Name = "Mock PMS",
                SourceType = "PMS",
                Url = "http://localhost:5095",
                IsEnabled = true
            };
            Property = new Property
            {
                Id = 1,
                Name = "StayHub Lake Resort",
                City = "Rottach-Egern",
                CountryCode = "DE"
            };

            SourceRepository.Sources.Add(Source);
            PropertyRepository.Properties.Add(Property);
            Manager = new SynchronizationManager(
                ExternalClient,
                RunRepository,
                SourceRepository,
                PropertyRepository,
                GuestRepository,
                ReservationRepository,
                ExecutionGate);
        }

        public Source Source { get; }
        public Property Property { get; }
        public FakeExternalReservationClient ExternalClient { get; } = new();
        public FakeSynchronizationRunRepository RunRepository { get; } = new();
        public FakeSourceRepository SourceRepository { get; } = new();
        public FakePropertyRepository PropertyRepository { get; } = new();
        public FakeGuestRepository GuestRepository { get; } = new();
        public FakeReservationRepository ReservationRepository { get; } = new();
        public SynchronizationExecutionGate ExecutionGate { get; } = new();
        public SynchronizationManager Manager { get; }

        public void AddExistingReservation(
            string externalId,
            DateOnly arrivalDate,
            DateOnly departureDate)
        {
            var guest = new Guest
            {
                Id = 100,
                FirstName = "Existing",
                LastName = "Guest",
                Email = "existing.guest@example.com"
            };
            GuestRepository.Guests.Add(guest);
            ReservationRepository.Reservations.Add(
                new Reservation
                {
                    Id = 100,
                    ExternalId = externalId,
                    SourceId = Source.Id,
                    Source = Source,
                    GuestId = guest.Id,
                    Guest = guest,
                    PropertyId = Property.Id,
                    Property = Property,
                    ArrivalDate = arrivalDate,
                    DepartureDate = departureDate,
                    Status = ReservationStatus.Confirmed
                });
        }
    }

    private sealed class FakeExternalReservationClient : IExternalReservationClient
    {
        public IReadOnlyList<ExternalReservationResponse> Reservations { get; set; } = [];
        public Exception? Exception { get; set; }
        public int RequestCount { get; private set; }

        public Task<IReadOnlyList<ExternalReservationResponse>> GetReservationsAsync(
            string sourceUrl,
            CancellationToken cancellationToken = default)
        {
            RequestCount++;

            if (Exception is not null)
                throw Exception;

            return Task.FromResult(Reservations);
        }
    }

    private sealed class FakeSynchronizationRunRepository : ISynchronizationRunRepository
    {
        public List<SynchronizationRun> Runs { get; } = [];

        public Task<IReadOnlyList<SynchronizationRun>> GetRecentAsync(
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SynchronizationRun>>(
                Runs.OrderByDescending(x => x.StartedAtUtc).Take(take).ToList());

        public Task<SynchronizationRun> AddAsync(
            SynchronizationRun run,
            CancellationToken cancellationToken = default)
        {
            run.Id = Runs.Count + 1;
            Runs.Add(run);
            return Task.FromResult(run);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeSourceRepository : ISourceRepository
    {
        public List<Source> Sources { get; } = [];

        public Task<IReadOnlyList<Source>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Source>>(Sources);

        public Task<Source?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Sources.SingleOrDefault(x => x.Id == id));

        public Task<Source> AddAsync(Source source, CancellationToken cancellationToken = default)
        {
            source.Id = Sources.Count + 1;
            Sources.Add(source);
            return Task.FromResult(source);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Delete(Source source) => Sources.Remove(source);
    }

    private sealed class FakePropertyRepository : IPropertyRepository
    {
        public List<Property> Properties { get; } = [];

        public Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Property>>(Properties);

        public Task<Property?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Properties.SingleOrDefault(x => x.Id == id));

        public Task<Property?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(Properties.SingleOrDefault(
                x => string.Equals(x.Name, name, StringComparison.Ordinal)));

        public Task<Property> AddAsync(Property property, CancellationToken cancellationToken = default)
        {
            property.Id = Properties.Count + 1;
            Properties.Add(property);
            return Task.FromResult(property);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Delete(Property property) => Properties.Remove(property);
    }

    private sealed class FakeGuestRepository : IGuestRepository
    {
        public List<Guest> Guests { get; } = [];

        public Task<IReadOnlyList<Guest>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Guest>>(Guests);

        public Task<Guest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Guests.SingleOrDefault(x => x.Id == id));

        public Task<Guest?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(Guests.SingleOrDefault(
                x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> EmailExistsAsync(
            string email,
            int? excludedGuestId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Guests.Any(
                x => x.Id != excludedGuestId
                     && string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> HasReservationsAsync(
            int guestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<Guest> AddAsync(Guest guest, CancellationToken cancellationToken = default)
        {
            guest.Id = Guests.Count == 0 ? 1 : Guests.Max(x => x.Id) + 1;
            Guests.Add(guest);
            return Task.FromResult(guest);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Delete(Guest guest) => Guests.Remove(guest);
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public List<Reservation> Reservations { get; } = [];

        public Task<PagedResult<Reservation>> GetAllAsync(
            ReservationQuery query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedResult<Reservation>(Reservations, Reservations.Count));

        public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Reservations.SingleOrDefault(x => x.Id == id));

        public Task<Reservation?> GetByExternalIdAsync(
            int sourceId,
            string externalId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Reservations.SingleOrDefault(
                x => x.SourceId == sourceId && x.ExternalId == externalId));

        public Task<bool> HasDateConflictAsync(
            int propertyId,
            DateOnly arrivalDate,
            DateOnly departureDate,
            int? excludedReservationId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Reservations.Any(
                x => x.PropertyId == propertyId
                     && x.Status != ReservationStatus.Cancelled
                     && x.ArrivalDate < departureDate
                     && arrivalDate < x.DepartureDate
                     && (!excludedReservationId.HasValue || x.Id != excludedReservationId.Value)));

        public Task<bool> ExternalIdExistsAsync(
            int sourceId,
            string externalId,
            int? excludedReservationId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Reservations.Any(
                x => x.SourceId == sourceId
                     && x.ExternalId == externalId
                     && (!excludedReservationId.HasValue || x.Id != excludedReservationId.Value)));

        public Task<Reservation> AddAsync(
            Reservation reservation,
            CancellationToken cancellationToken = default)
        {
            reservation.Id = Reservations.Count == 0 ? 1 : Reservations.Max(x => x.Id) + 1;
            Reservations.Add(reservation);
            return Task.FromResult(reservation);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Delete(Reservation reservation) => Reservations.Remove(reservation);
    }
}
