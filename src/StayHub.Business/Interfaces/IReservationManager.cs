using StayHub.Business.Results;
using StayHub.Contracts.Reservations;
using StayHub.Contracts.Common;

namespace StayHub.Business.Interfaces;

public interface IReservationManager
{
    Task<PagedResponse<ReservationResponse>> GetAllAsync(
        ReservationQueryRequest query,
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
