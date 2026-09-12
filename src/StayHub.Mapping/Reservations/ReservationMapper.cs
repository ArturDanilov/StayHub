using StayHub.Contracts.Reservations;
using StayHub.Domain.Models;
using StayHub.Mapping.Guests;

namespace StayHub.Mapping.Reservations;

public static class ReservationMapper
{
    public static Reservation ToDomain(CreateReservationRequest request)
    {
        return new Reservation
        {
            ExternalId = request.ExternalId.Trim(),
            SourceId = request.SourceId,
            GuestId = request.GuestId,
            ArrivalDate = request.ArrivalDate,
            DepartureDate = request.DepartureDate,
            PropertyId = request.PropertyId,
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static void MapToDomain(
        UpdateReservationRequest request,
        Reservation reservation)
    {
        reservation.ExternalId = request.ExternalId.Trim();
        reservation.SourceId = request.SourceId;
        reservation.GuestId = request.GuestId;
        reservation.ArrivalDate = request.ArrivalDate;
        reservation.DepartureDate = request.DepartureDate;
        reservation.PropertyId = request.PropertyId;
    }

    public static ReservationResponse ToResponse(Reservation reservation)
    {
        return new ReservationResponse(
            reservation.Id,
            reservation.ExternalId,
            reservation.SourceId,
            reservation.Source.Name,
            reservation.ArrivalDate,
            reservation.DepartureDate,
            ToContract(reservation.Status),
            reservation.PropertyId,
            reservation.Property.Name,
            GuestMapper.ToResponse(reservation.Guest),
            reservation.CreatedAtUtc);
    }
    
    public static ReservationStatus ToDomain(ReservationStatusContract status)
    {
        return status switch
        {
            ReservationStatusContract.Confirmed =>
                ReservationStatus.Confirmed,

            ReservationStatusContract.CheckedIn =>
                ReservationStatus.CheckedIn,

            ReservationStatusContract.CheckedOut =>
                ReservationStatus.CheckedOut,

            ReservationStatusContract.Cancelled =>
                ReservationStatus.Cancelled,

            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported reservation status.")
        };
    }

    public static ReservationStatusContract ToContract(ReservationStatus status)
    {
        return status switch
        {
            ReservationStatus.Confirmed =>
                ReservationStatusContract.Confirmed,

            ReservationStatus.CheckedIn =>
                ReservationStatusContract.CheckedIn,

            ReservationStatus.CheckedOut =>
                ReservationStatusContract.CheckedOut,

            ReservationStatus.Cancelled =>
                ReservationStatusContract.Cancelled,

            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported reservation status.")
        };
    }
}
