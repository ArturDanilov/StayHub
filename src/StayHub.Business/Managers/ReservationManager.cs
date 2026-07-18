using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Contracts.Reservations;
using StayHub.Domain.Models;
using StayHub.Mapping.Reservations;

namespace StayHub.Business.Managers;

public sealed class ReservationManager(
    IReservationRepository reservationRepository,
    IPropertyRepository propertyRepository,
    IGuestRepository guestRepository)
    : IReservationManager
{
    public async Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var reservations =
            await reservationRepository.GetAllAsync(cancellationToken);

        return reservations
            .Select(ReservationMapper.ToResponse)
            .ToList();
    }

    public async Task<ReservationResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation =
            await reservationRepository.GetByIdAsync(
                id,
                cancellationToken);

        return reservation is null
            ? null
            : ReservationMapper.ToResponse(reservation);
    }

    public async Task<OperationResult<ReservationResponse, ReservationError>> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(
                request.ArrivalDate,
                request.DepartureDate))
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.InvalidDateRange);
        }

        var property = await propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.InvalidDateRange);
        }

        var guest = await guestRepository.GetByIdAsync(
            request.GuestId,
            cancellationToken);

        if (guest is null)
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.InvalidDateRange);
        }

        var externalIdExists =
            await reservationRepository.ExternalIdExistsAsync(
                request.ExternalId.Trim(),
                cancellationToken);

        if (externalIdExists)
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.InvalidDateRange);
        }

        var reservation = ReservationMapper.ToDomain(request);

        reservation.Property = property;
        reservation.Guest = guest;

        var created = await reservationRepository.AddAsync(
            reservation,
            cancellationToken);

        return OperationResult<ReservationResponse, ReservationError>.Success(
            ReservationMapper.ToResponse(created));
    }

    public async Task<ReservationError> UpdateAsync(
        int id,
        UpdateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(
                request.ArrivalDate,
                request.DepartureDate))
        {
            return ReservationError.InvalidDateRange;
        }

        var reservation =
            await reservationRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (reservation is null)
            return ReservationError.ReservationNotFound;

        var property = await propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            return ReservationError.PropertyNotFound;

        var guest = await guestRepository.GetByIdAsync(
            request.GuestId,
            cancellationToken);

        if (guest is null)
            return ReservationError.GuestNotFound;

        ReservationMapper.MapToDomain(request, reservation);

        reservation.Property = property;
        reservation.Guest = guest;

        await reservationRepository.SaveChangesAsync(
            cancellationToken);

        return ReservationError.None;
    }

    public async Task<ReservationError> UpdateStatusAsync(
        int id,
        UpdateReservationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await reservationRepository.GetByIdAsync(
            id,
            cancellationToken);
    
        if (reservation is null)
            return ReservationError.ReservationNotFound;
    
        var nextStatus = ReservationMapper.ToDomain(request.Status);
    
        if (!CanTransition(reservation.Status, nextStatus))
            return ReservationError.InvalidStatusTransition;
    
        reservation.Status = nextStatus;
    
        await reservationRepository.SaveChangesAsync(cancellationToken);
    
        return ReservationError.None;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation =
            await reservationRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (reservation is null)
            return false;

        reservationRepository.Delete(reservation);

        await reservationRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static bool IsDateRangeValid(
        DateOnly arrivalDate,
        DateOnly departureDate)
    {
        return departureDate > arrivalDate;
    }

    private static bool CanTransition(
        ReservationStatus current,
        ReservationStatus next)
    {
        return current switch
        {
            ReservationStatus.Confirmed =>
                next is ReservationStatus.CheckedIn
                    or ReservationStatus.Cancelled,

            ReservationStatus.CheckedIn =>
                next == ReservationStatus.CheckedOut,

            ReservationStatus.CheckedOut => false,
            ReservationStatus.Cancelled => false,

            _ => false
        };
    }
}
