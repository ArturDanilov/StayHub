using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Reservations;

public sealed record CreateReservationRequest(
    [Required, StringLength(100)] string ExternalId,
    [Range(1, int.MaxValue)] int SourceId,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    [Range(1, int.MaxValue)] int PropertyId,
    [Range(1, int.MaxValue)] int GuestId);
