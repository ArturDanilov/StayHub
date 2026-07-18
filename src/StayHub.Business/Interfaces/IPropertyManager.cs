using StayHub.Contracts.Properties;

namespace StayHub.Business.Interfaces;

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
