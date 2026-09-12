using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Dal.Data;
using StayHub.Domain.Models;
using StayHub.Business.Models;

namespace StayHub.Dal.Repositories;

public sealed class ReservationRepository(StayHubDbContext dbContext)
    : IReservationRepository
{
    public async Task<PagedResult<Reservation>> GetAllAsync(
        ReservationQuery query,
        CancellationToken cancellationToken = default)
    {
        var reservations = dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.Source)
            .Include(x => x.Property)
            .Include(x => x.Guest)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search;
            reservations = reservations.Where(x =>
                x.ExternalId.Contains(search)
                || x.Guest.FirstName.Contains(search)
                || x.Guest.LastName.Contains(search)
                || x.Guest.Email.Contains(search)
                || x.Property.Name.Contains(search)
                || x.Source.Name.Contains(search));
        }

        if (query.Status.HasValue)
            reservations = reservations.Where(x => x.Status == query.Status.Value);
        if (query.ArrivalFrom.HasValue)
            reservations = reservations.Where(x => x.ArrivalDate >= query.ArrivalFrom.Value);
        if (query.ArrivalTo.HasValue)
            reservations = reservations.Where(x => x.ArrivalDate <= query.ArrivalTo.Value);
        if (query.PropertyId.HasValue)
            reservations = reservations.Where(x => x.PropertyId == query.PropertyId.Value);
        if (query.SourceId.HasValue)
            reservations = reservations.Where(x => x.SourceId == query.SourceId.Value);

        var totalCount = await reservations.CountAsync(cancellationToken);
        reservations = ApplySorting(reservations, query);
        var items = await reservations
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Reservation>(items, totalCount);
    }

    private static IQueryable<Reservation> ApplySorting(
        IQueryable<Reservation> reservations,
        ReservationQuery query)
    {
        return (query.SortBy, query.Descending) switch
        {
            (ReservationSortField.DepartureDate, false) => reservations.OrderBy(x => x.DepartureDate).ThenBy(x => x.Id),
            (ReservationSortField.DepartureDate, true) => reservations.OrderByDescending(x => x.DepartureDate).ThenByDescending(x => x.Id),
            (ReservationSortField.GuestName, false) => reservations.OrderBy(x => x.Guest.LastName).ThenBy(x => x.Guest.FirstName).ThenBy(x => x.Id),
            (ReservationSortField.GuestName, true) => reservations.OrderByDescending(x => x.Guest.LastName).ThenByDescending(x => x.Guest.FirstName).ThenByDescending(x => x.Id),
            (ReservationSortField.PropertyName, false) => reservations.OrderBy(x => x.Property.Name).ThenBy(x => x.Id),
            (ReservationSortField.PropertyName, true) => reservations.OrderByDescending(x => x.Property.Name).ThenByDescending(x => x.Id),
            (ReservationSortField.CreatedAt, false) => reservations.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id),
            (ReservationSortField.CreatedAt, true) => reservations.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id),
            (_, false) => reservations.OrderBy(x => x.ArrivalDate).ThenBy(x => x.Id),
            _ => reservations.OrderByDescending(x => x.ArrivalDate).ThenByDescending(x => x.Id)
        };
    }

    public Task<Reservation?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reservations
            .Include(x => x.Source)
            .Include(x => x.Property)
            .Include(x => x.Guest)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Reservation?> GetByExternalIdAsync(
        int sourceId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reservations
            .Include(x => x.Source)
            .Include(x => x.Property)
            .Include(x => x.Guest)
            .FirstOrDefaultAsync(
                x => x.SourceId == sourceId && x.ExternalId == externalId,
                cancellationToken);
    }

    public Task<bool> HasDateConflictAsync(
        int propertyId,
        DateOnly arrivalDate,
        DateOnly departureDate,
        int? excludedReservationId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reservations.AnyAsync(
            x => x.PropertyId == propertyId
                 && x.Status != ReservationStatus.Cancelled
                 && x.ArrivalDate < departureDate
                 && arrivalDate < x.DepartureDate
                 && (!excludedReservationId.HasValue || x.Id != excludedReservationId.Value),
            cancellationToken);
    }

    public Task<bool> ExternalIdExistsAsync(
        int sourceId,
        string externalId,
        int? excludedReservationId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reservations
            .AnyAsync(
                x => x.SourceId == sourceId
                     && x.ExternalId == externalId
                     && (!excludedReservationId.HasValue || x.Id != excludedReservationId.Value),
                cancellationToken);
    }

    public async Task<Reservation> AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Reservations.AddAsync(
            reservation,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return reservation;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Delete(Reservation reservation)
    {
        dbContext.Reservations.Remove(reservation);
    }
}
