using System.ComponentModel.DataAnnotations;
using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Reservations;
using StayHub.Contracts.Synchronization;
using StayHub.Domain.Models;

namespace StayHub.Business.Managers;

public sealed class SynchronizationManager(
    IExternalReservationClient externalClient,
    ISynchronizationRunRepository runRepository,
    ISourceRepository sourceRepository,
    IPropertyRepository propertyRepository,
    IGuestRepository guestRepository,
    IReservationRepository reservationRepository)
    : ISynchronizationManager
{
    public async Task<OperationResult<SynchronizationRunResponse, SynchronizationError>> SynchronizeAsync(
        int sourceId,
        CancellationToken cancellationToken = default)
    {
        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);
        if (source is null)
            return Failure(SynchronizationError.SourceNotFound);
        if (!source.IsEnabled)
            return Failure(SynchronizationError.SourceDisabled);
        if (string.IsNullOrWhiteSpace(source.Url))
            return Failure(SynchronizationError.SourceUrlMissing);

        var run = await runRepository.AddAsync(
            new SynchronizationRun
            {
                SourceId = source.Id,
                Source = source,
                Status = SynchronizationStatus.Running,
                StartedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        try
        {
            var externalReservations = await externalClient.GetReservationsAsync(
                source.Url,
                cancellationToken);

            var errors = new List<string>();
            foreach (var external in externalReservations)
            {
                try
                {
                    await SynchronizeReservationAsync(source, external, run, cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    run.FailedCount++;
                    errors.Add($"{external.ExternalId}: {exception.Message}");
                }
            }

            run.Status = run.FailedCount == 0
                ? SynchronizationStatus.Completed
                : SynchronizationStatus.CompletedWithErrors;
            run.ErrorMessage = JoinErrors(errors);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.Status = SynchronizationStatus.Failed;
            run.ErrorMessage = Truncate(exception.Message);
        }
        catch (OperationCanceledException)
        {
            run.Status = SynchronizationStatus.Failed;
            run.ErrorMessage = "Synchronization was cancelled.";
            run.CompletedAtUtc = DateTime.UtcNow;
            await runRepository.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        run.CompletedAtUtc = DateTime.UtcNow;
        await runRepository.SaveChangesAsync(cancellationToken);

        return OperationResult<SynchronizationRunResponse, SynchronizationError>.Success(Map(run));
    }

    public async Task<IReadOnlyList<SynchronizationRunResponse>> GetRecentRunsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var runs = await runRepository.GetRecentAsync(Math.Clamp(take, 1, 100), cancellationToken);
        return runs.Select(Map).ToList();
    }

    private async Task SynchronizeReservationAsync(
        Source source,
        ExternalReservationResponse external,
        SynchronizationRun run,
        CancellationToken cancellationToken)
    {
        Validate(external);

        var propertyName = external.PropertyName.Trim();
        var property = await propertyRepository.GetByNameAsync(propertyName, cancellationToken)
                       ?? throw new InvalidOperationException($"Property '{propertyName}' was not found.");

        var email = external.GuestEmail.Trim().ToLowerInvariant();
        var guest = await guestRepository.GetByEmailAsync(email, cancellationToken);
        if (guest is null)
        {
            guest = await guestRepository.AddAsync(
                new Guest
                {
                    FirstName = external.GuestFirstName.Trim(),
                    LastName = external.GuestLastName.Trim(),
                    Email = email,
                    Phone = NormalizeOptional(external.GuestPhone),
                    CreatedAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }

        var externalId = external.ExternalId.Trim();
        var reservation = await reservationRepository.GetByExternalIdAsync(
            source.Id,
            externalId,
            cancellationToken);

        if (external.Status != ReservationStatusContract.Cancelled
            && await reservationRepository.HasDateConflictAsync(
                property.Id,
                external.ArrivalDate,
                external.DepartureDate,
                reservation?.Id,
                cancellationToken))
        {
            run.ConflictCount++;
        }

        var status = MapStatus(external.Status);
        if (reservation is null)
        {
            await reservationRepository.AddAsync(
                new Reservation
                {
                    ExternalId = externalId,
                    SourceId = source.Id,
                    Source = source,
                    GuestId = guest.Id,
                    Guest = guest,
                    PropertyId = property.Id,
                    Property = property,
                    ArrivalDate = external.ArrivalDate,
                    DepartureDate = external.DepartureDate,
                    Status = status,
                    CreatedAtUtc = DateTime.UtcNow
                },
                cancellationToken);
            run.CreatedCount++;
            return;
        }

        if (reservation.GuestId == guest.Id
            && reservation.PropertyId == property.Id
            && reservation.ArrivalDate == external.ArrivalDate
            && reservation.DepartureDate == external.DepartureDate
            && reservation.Status == status)
        {
            run.UnchangedCount++;
            return;
        }

        reservation.GuestId = guest.Id;
        reservation.Guest = guest;
        reservation.PropertyId = property.Id;
        reservation.Property = property;
        reservation.ArrivalDate = external.ArrivalDate;
        reservation.DepartureDate = external.DepartureDate;
        reservation.Status = status;
        await reservationRepository.SaveChangesAsync(cancellationToken);
        run.UpdatedCount++;
    }

    private static void Validate(ExternalReservationResponse reservation)
    {
        if (string.IsNullOrWhiteSpace(reservation.ExternalId))
            throw new InvalidOperationException("External ID is required.");
        if (string.IsNullOrWhiteSpace(reservation.PropertyName))
            throw new InvalidOperationException("Property name is required.");
        if (string.IsNullOrWhiteSpace(reservation.GuestFirstName)
            || string.IsNullOrWhiteSpace(reservation.GuestLastName)
            || string.IsNullOrWhiteSpace(reservation.GuestEmail))
            throw new InvalidOperationException("Guest name and email are required.");
        if (!new EmailAddressAttribute().IsValid(reservation.GuestEmail))
            throw new InvalidOperationException("Guest email is invalid.");
        if (reservation.DepartureDate <= reservation.ArrivalDate)
            throw new InvalidOperationException("Departure date must be after arrival date.");
        if (!Enum.IsDefined(reservation.Status))
            throw new InvalidOperationException("Reservation status is invalid.");
    }

    private static ReservationStatus MapStatus(ReservationStatusContract status) => status switch
    {
        ReservationStatusContract.CheckedIn => ReservationStatus.CheckedIn,
        ReservationStatusContract.CheckedOut => ReservationStatus.CheckedOut,
        ReservationStatusContract.Cancelled => ReservationStatus.Cancelled,
        _ => ReservationStatus.Confirmed
    };

    private static SynchronizationRunResponse Map(SynchronizationRun run) => new(
        run.Id,
        run.SourceId,
        run.Source.Name,
        run.Status.ToString(),
        run.StartedAtUtc,
        run.CompletedAtUtc,
        run.CreatedCount,
        run.UpdatedCount,
        run.UnchangedCount,
        run.ConflictCount,
        run.FailedCount,
        run.ErrorMessage);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? JoinErrors(IEnumerable<string> errors)
    {
        var value = string.Join(Environment.NewLine, errors);
        return string.IsNullOrWhiteSpace(value) ? null : Truncate(value);
    }

    private static string Truncate(string value) =>
        value.Length <= 2000 ? value : value[..2000];

    private static OperationResult<SynchronizationRunResponse, SynchronizationError> Failure(
        SynchronizationError error) =>
        OperationResult<SynchronizationRunResponse, SynchronizationError>.Failure(error);
}
