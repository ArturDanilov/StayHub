using StayHub.Business.Interfaces;
using StayHub.Business.Models;
using StayHub.Contracts.Operations;
using StayHub.Mapping.Reservations;

namespace StayHub.Business.Managers;

public sealed class OperationsDashboardManager(
    IOperationsDashboardRepository repository,
    TimeProvider timeProvider)
    : IOperationsDashboardManager
{
    private const int PreviewLimit = 5;
    private const int UpcomingDays = 7;
    private static readonly TimeZoneInfo BusinessTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

    public async Task<OperationsDashboardResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var businessDate = GetBusinessDate();
        var data = await repository.GetAsync(
            businessDate,
            businessDate.AddDays(UpcomingDays),
            PreviewLimit,
            cancellationToken);

        return new OperationsDashboardResponse(
            businessDate,
            MapSection(data.Arrivals),
            MapSection(data.Departures),
            MapSection(data.InHouse),
            MapSection(data.UpcomingArrivals),
            data.SynchronizationAlerts.Select(MapAlert).ToList());
    }

    private DateOnly GetBusinessDate()
    {
        var localNow = TimeZoneInfo.ConvertTime(
            timeProvider.GetUtcNow(),
            BusinessTimeZone);

        return DateOnly.FromDateTime(localNow.DateTime);
    }

    private static DashboardSection<DashboardBookingResponse> MapSection(
        DashboardSectionResult<DashboardBooking> section)
    {
        return new DashboardSection<DashboardBookingResponse>(
            section.TotalCount,
            section.Items.Select(MapBooking).ToList());
    }

    private static DashboardBookingResponse MapBooking(DashboardBooking booking)
    {
        return new DashboardBookingResponse(
            booking.Id,
            booking.ArrivalDate,
            booking.DepartureDate,
            ReservationMapper.ToContract(booking.Status),
            booking.PropertyId,
            booking.PropertyName,
            booking.GuestId,
            booking.GuestFirstName,
            booking.GuestLastName);
    }

    private static SynchronizationAlertResponse MapAlert(
        DashboardSynchronizationAlert alert)
    {
        return new SynchronizationAlertResponse(
            alert.Id,
            alert.SourceId,
            alert.SourceName,
            alert.Status.ToString(),
            alert.StartedAtUtc,
            alert.ConflictCount,
            alert.FailedCount,
            alert.ErrorMessage);
    }
}
