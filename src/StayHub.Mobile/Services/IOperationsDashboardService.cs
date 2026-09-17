using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface IOperationsDashboardService
{
    Task<OperationsDashboardOverview> GetAsync(
        CancellationToken cancellationToken = default);
}
