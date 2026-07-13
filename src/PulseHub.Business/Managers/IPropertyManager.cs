using PulseHub.Contracts.Properties;

namespace PulseHub.Business.Managers;

public interface IPropertyManager
{
    Task<IReadOnlyList<PropertyResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PropertyResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PropertyResponse> CreateAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
