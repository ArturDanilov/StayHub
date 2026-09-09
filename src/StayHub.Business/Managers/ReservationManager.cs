using StayHub.Business.Interfaces;
using StayHub.Business.Results;
using StayHub.Business.Models;
using StayHub.Contracts.Common;
using StayHub.Contracts.Reservations;
using StayHub.Domain.Models;
using StayHub.Mapping.Reservations;

namespace StayHub.Business.Managers;

public sealed class ReservationManager(
    IReservationRepository reservationRepository,
    IPropertyRepository propertyRepository,
    IGuestRepository guestRepository,
    ISourceRepository sourceRepository)
    : IReservationManager
{
    public async Task<PagedResponse<ReservationResponse>> GetAllAsync(
        ReservationQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var reservations =
            await reservationRepository.GetAllAsync(MapQuery(query), cancellationToken);

        var items = reservations.Items
            .Select(ReservationMapper.ToResponse)
            .ToList();

        return new PagedResponse<ReservationResponse>(
            items,
            query.Page,
            query.PageSize,
            reservations.TotalCount);
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
                ReservationError.PropertyNotFound);
        }

        var guest = await guestRepository.GetByIdAsync(
            request.GuestId,
            cancellationToken);

        if (guest is null)
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.GuestNotFound);
        }

        var source = await sourceRepository.GetByIdAsync(
            request.SourceId,
            cancellationToken);

        if (source is null)
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.SourceNotFound);
        }

        var externalIdExists =
            await reservationRepository.ExternalIdExistsAsync(
                request.SourceId,
                request.ExternalId.Trim(),
                null,
                cancellationToken);

        if (externalIdExists)
        {
            return OperationResult<ReservationResponse, ReservationError>.Failure(
                ReservationError.ExternalIdAlreadyExists);
        }

        var reservation = ReservationMapper.ToDomain(request);

        reservation.Property = property;
        reservation.Guest = guest;
        reservation.Source = source;

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

        var source = await sourceRepository.GetByIdAsync(
            request.SourceId,
            cancellationToken);

        if (source is null)
            return ReservationError.SourceNotFound;

        var externalIdExists = await reservationRepository.ExternalIdExistsAsync(
            request.SourceId,
            request.ExternalId.Trim(),
            id,
            cancellationToken);

        if (externalIdExists)
            return ReservationError.ExternalIdAlreadyExists;

        ReservationMapper.MapToDomain(request, reservation);

        reservation.Property = property;
        reservation.Guest = guest;
        reservation.Source = source;

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

    private static ReservationQuery MapQuery(ReservationQueryRequest query)
    {
        return new ReservationQuery(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.Status.HasValue ? ReservationMapper.ToDomain(query.Status.Value) : null,
            query.ArrivalFrom,
            query.ArrivalTo,
            query.PropertyId,
            query.SourceId,
            query.SortBy switch
            {
                ReservationSortBy.DepartureDate => ReservationSortField.DepartureDate,
                ReservationSortBy.GuestName => ReservationSortField.GuestName,
                ReservationSortBy.PropertyName => ReservationSortField.PropertyName,
                ReservationSortBy.CreatedAt => ReservationSortField.CreatedAt,
                _ => ReservationSortField.ArrivalDate
            },
            query.SortDirection == SortDirection.Descending,
            query.Page,
            query.PageSize);
    }
}
