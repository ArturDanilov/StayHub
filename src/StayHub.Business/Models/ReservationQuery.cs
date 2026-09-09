using StayHub.Domain.Models;

namespace StayHub.Business.Models;

public sealed record ReservationQuery(
    string? Search,
    ReservationStatus? Status,
    DateOnly? ArrivalFrom,
    DateOnly? ArrivalTo,
    int? PropertyId,
    int? SourceId,
    ReservationSortField SortBy,
    bool Descending,
    int Page,
    int PageSize);

public enum ReservationSortField
{
    ArrivalDate,
    DepartureDate,
    GuestName,
    PropertyName,
    CreatedAt
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
