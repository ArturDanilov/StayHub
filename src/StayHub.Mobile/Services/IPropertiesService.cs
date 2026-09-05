using StayHub.Mobile.Models;

namespace StayHub.Mobile.Services;

public interface IPropertiesService
{
    Task<IReadOnlyList<PropertyOverview>> GetAllAsync(CancellationToken cancellationToken = default);
}
