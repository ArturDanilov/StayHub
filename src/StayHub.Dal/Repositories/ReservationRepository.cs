using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class ReservationRepository(StayHubDbContext dbContext)
    : IReservationRepository
{
    public async Task<IReadOnlyList<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.Source)
            .Include(x => x.Property)
            .Include(x => x.Guest)
            .OrderBy(x => x.ArrivalDate)
            .ToListAsync(cancellationToken);
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
