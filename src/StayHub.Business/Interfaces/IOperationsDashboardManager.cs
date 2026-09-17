using StayHub.Contracts.Operations;

namespace StayHub.Business.Interfaces;

public interface IOperationsDashboardManager
{
    Task<OperationsDashboardResponse> GetAsync(
        CancellationToken cancellationToken = default);
}
