using StayHub.Business.Models;

namespace StayHub.Business.Interfaces;

public interface IOperationsDashboardRepository
{
    Task<OperationsDashboardData> GetAsync(
        DateOnly businessDate,
        DateOnly upcomingThrough,
        int previewLimit,
        CancellationToken cancellationToken = default);
}
