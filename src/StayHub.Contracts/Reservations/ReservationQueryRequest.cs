using System.ComponentModel.DataAnnotations;

namespace StayHub.Contracts.Reservations;

public sealed class ReservationQueryRequest
{
    [StringLength(100)]
    public string? Search { get; init; }

    public ReservationStatusContract? Status { get; init; }
    
    public DateOnly? ArrivalFrom { get; init; }
    
    public DateOnly? ArrivalTo { get; init; }

    [Range(1, int.MaxValue)]
    public int? PropertyId { get; init; }

    [Range(1, int.MaxValue)]
    public int? SourceId { get; init; }

    public ReservationSortBy SortBy { get; init; } = ReservationSortBy.ArrivalDate;
    
    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public enum ReservationSortBy
{
    ArrivalDate,
    DepartureDate,
    GuestName,
    PropertyName,
    CreatedAt
}

public enum SortDirection
{
    Ascending,
    Descending
}
