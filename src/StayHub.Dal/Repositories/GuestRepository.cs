using Microsoft.EntityFrameworkCore;
using StayHub.Business.Interfaces;
using StayHub.Dal.Data;
using StayHub.Domain.Models;

namespace StayHub.Dal.Repositories;

public sealed class GuestRepository(StayHubDbContext dbContext)
    : IGuestRepository
{
    public async Task<IReadOnlyList<Guest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Guests
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);
    }

    public Task<Guest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Guests
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        int? excludedGuestId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Guests.AnyAsync(
            x => x.Email == email &&
                 (!excludedGuestId.HasValue || x.Id != excludedGuestId.Value),
            cancellationToken);
    }

    public Task<bool> HasReservationsAsync(
        int guestId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reservations
            .AnyAsync(
                x => x.GuestId == guestId,
                cancellationToken);
    }
    
    public async Task<Guest> AddAsync(
        Guest guest,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Guests.AddAsync(guest, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return guest;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Delete(Guest guest)
    {
        dbContext.Guests.Remove(guest);
    }
}
