using StayHub.Business.Results;
using StayHub.Contracts.Guests;

namespace StayHub.Business.Interfaces;

public interface IGuestManager
{
    Task<IReadOnlyList<GuestResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<GuestResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OperationResult<GuestResponse, GuestError>> CreateAsync(
        CreateGuestRequest request,
        CancellationToken cancellationToken = default);

    Task<GuestError> UpdateAsync(
        int id,
        UpdateGuestRequest request,
        CancellationToken cancellationToken = default);

    Task<GuestError> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
