using System.ComponentModel.DataAnnotations;
using StayHub.Contracts.Reservations;

namespace StayHub.Contracts.Synchronization;

public sealed record ExternalReservationResponse(
    [Required, StringLength(100)] string ExternalId,
    [Required, StringLength(200)] string PropertyName,
    [Required, StringLength(100)] string GuestFirstName,
    [Required, StringLength(100)] string GuestLastName,
    [Required, EmailAddress, StringLength(200)] string GuestEmail,
    [StringLength(50)] string? GuestPhone,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    ReservationStatusContract Status);
