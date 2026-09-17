using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Business.Models;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class OperationsDashboardRepository(StayHubDbContext dbContext)
    : IOperationsDashboardRepository
{
    public async Task<OperationsDashboardData> GetAsync(
        DateOnly businessDate,
        DateOnly upcomingThrough,
        int previewLimit,
        CancellationToken cancellationToken = default)
    {
        var reservations = dbContext.Reservations.AsNoTracking();

        var arrivals = await GetSectionAsync(
            reservations.Where(x =>
                x.ArrivalDate == businessDate
                && x.Status != ReservationStatus.Cancelled),
            query => query
                .OrderBy(x => x.ArrivalDate)
                .ThenBy(x => x.Guest.LastName)
                .ThenBy(x => x.Guest.FirstName)
                .ThenBy(x => x.Id),
            previewLimit,
            cancellationToken);

        var departures = await GetSectionAsync(
            reservations.Where(x =>
                x.DepartureDate == businessDate
                && x.Status != ReservationStatus.Cancelled),
            query => query
                .OrderBy(x => x.DepartureDate)
                .ThenBy(x => x.Guest.LastName)
                .ThenBy(x => x.Guest.FirstName)
                .ThenBy(x => x.Id),
            previewLimit,
            cancellationToken);

        var inHouse = await GetSectionAsync(
            reservations.Where(x => x.Status == ReservationStatus.CheckedIn),
            query => query
                .OrderBy(x => x.DepartureDate)
                .ThenBy(x => x.Guest.LastName)
                .ThenBy(x => x.Guest.FirstName)
                .ThenBy(x => x.Id),
            previewLimit,
            cancellationToken);

        var upcomingArrivals = await GetSectionAsync(
            reservations.Where(x =>
                x.ArrivalDate > businessDate
                && x.ArrivalDate <= upcomingThrough
                && x.Status == ReservationStatus.Confirmed),
            query => query
                .OrderBy(x => x.ArrivalDate)
                .ThenBy(x => x.Guest.LastName)
                .ThenBy(x => x.Guest.FirstName)
                .ThenBy(x => x.Id),
            previewLimit,
            cancellationToken);

        var alerts = await dbContext.SynchronizationRuns
            .AsNoTracking()
            .Where(x =>
                x.Status == SynchronizationStatus.Failed
                || x.Status == SynchronizationStatus.CompletedWithErrors
                || x.ConflictCount > 0
                || x.FailedCount > 0)
            .OrderByDescending(x => x.StartedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(previewLimit)
            .Select(x => new DashboardSynchronizationAlert(
                x.Id,
                x.SourceId,
                x.Source.Name,
                x.Status,
                x.StartedAtUtc,
                x.ConflictCount,
                x.FailedCount,
                x.ErrorMessage))
            .ToListAsync(cancellationToken);

        return new OperationsDashboardData(
            arrivals,
            departures,
            inHouse,
            upcomingArrivals,
            alerts);
    }

    private static async Task<DashboardSectionResult<DashboardBooking>> GetSectionAsync(
        IQueryable<Reservation> query,
        Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> applyOrdering,
        int previewLimit,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await applyOrdering(query)
            .Take(previewLimit)
            .Select(x => new DashboardBooking(
                x.Id,
                x.ArrivalDate,
                x.DepartureDate,
                x.Status,
                x.PropertyId,
                x.Property.Name,
                x.GuestId,
                x.Guest.FirstName,
                x.Guest.LastName))
            .ToListAsync(cancellationToken);

        return new DashboardSectionResult<DashboardBooking>(totalCount, items);
    }
}
