using StayHub.Contracts.Reservations;

namespace StayHub.Mobile.Models;

public sealed class ReservationSearchCriteria
{
    public string? Search { get; set; }
    public ReservationStatusContract? Status { get; set; }
    public DateOnly? ArrivalFrom { get; set; }
    public DateOnly? ArrivalTo { get; set; }
    public int? PropertyId { get; set; }
    public int? SourceId { get; set; }
    public ReservationSortBy SortBy { get; set; } = ReservationSortBy.ArrivalDate;
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed record PagedReservationOverview(
    IReadOnlyList<ReservationOverview> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
