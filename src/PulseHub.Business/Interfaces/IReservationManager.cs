using PulseHub.Business.Results;
using PulseHub.Contracts.Reservations;

namespace PulseHub.Business.Interfaces;

public interface IReservationManager
{
    Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ReservationResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OperationResult<ReservationResponse, ReservationError>> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<ReservationError> UpdateAsync(
        int id,
        UpdateReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<ReservationError> UpdateStatusAsync(
        int id,
        UpdateReservationStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
