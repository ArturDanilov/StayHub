using PulseHub.Business.Interfaces;
using PulseHub.Business.Results;
using PulseHub.Contracts.Guests;
using PulseHub.Mapping.Guests;

namespace PulseHub.Business.Managers;

public sealed class GuestManager(IGuestRepository repository)
    : IGuestManager
{
    public async Task<IReadOnlyList<GuestResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var guests = await repository.GetAllAsync(cancellationToken);

        return guests
            .Select(GuestMapper.ToResponse)
            .ToList();
    }

    public async Task<GuestResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var guest = await repository.GetByIdAsync(
            id,
            cancellationToken);

        return guest is null
            ? null
            : GuestMapper.ToResponse(guest);
    }

    public async Task<OperationResult<GuestResponse, GuestError>> CreateAsync(
        CreateGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        var emailExists = await repository.EmailExistsAsync(
            normalizedEmail,
            cancellationToken: cancellationToken);

        if (emailExists)
        {
            return OperationResult<GuestResponse, GuestError>.Failure(
                GuestError.EmailAlreadyExists);
        }

        var guest = GuestMapper.ToDomain(request);

        var created = await repository.AddAsync(
            guest,
            cancellationToken);

        return OperationResult<GuestResponse, GuestError>.Success(
            GuestMapper.ToResponse(created));
    }

    public async Task<GuestError> UpdateAsync(
        int id,
        UpdateGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        var guest = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (guest is null)
            return GuestError.GuestNotFound;

        var normalizedEmail = NormalizeEmail(request.Email);

        var emailExists = await repository.EmailExistsAsync(
            normalizedEmail,
            excludedGuestId: id,
            cancellationToken: cancellationToken);

        if (emailExists)
            return GuestError.EmailAlreadyExists;

        GuestMapper.MapToDomain(request, guest);

        await repository.SaveChangesAsync(cancellationToken);

        return GuestError.None;
    }

    public async Task<GuestError> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var guest = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (guest is null)
            return GuestError.GuestNotFound;

        var hasReservations = await repository.HasReservationsAsync(
            id,
            cancellationToken);

        if (hasReservations)
            return GuestError.GuestHasReservations;

        repository.Delete(guest);

        await repository.SaveChangesAsync(cancellationToken);

        return GuestError.None;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
