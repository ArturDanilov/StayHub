using StayHub.Domain.Models;

namespace StayHub.Business.Interfaces;

public interface IGuestRepository
{
    Task<IReadOnlyList<Guest>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Guest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludedGuestId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasReservationsAsync(
        int guestId,
        CancellationToken cancellationToken = default);

    Task<Guest> AddAsync(
        Guest guest,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    void Delete(Guest guest);
}
