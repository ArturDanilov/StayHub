namespace PulseHub.Domain.Models;

public class Reservation
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string GuestName { get; set; } = string.Empty;

    public DateOnly ArrivalDate { get; set; }

    public DateOnly DepartureDate { get; set; }

    public ReservationStatus Status { get; set; }

    public int PropertyId { get; set; }

    public Property Property { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
}