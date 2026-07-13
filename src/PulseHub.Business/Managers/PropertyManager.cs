using PulseHub.Business.Repositories;
using PulseHub.Contracts.Properties;
using PulseHub.Mapping.Properties;

namespace PulseHub.Business.Managers;

public sealed class PropertyManager(IPropertyRepository repository)
    : IPropertyManager
{
    public async Task<IReadOnlyList<PropertyResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var properties = await repository.GetAllAsync(cancellationToken);

        return properties
            .Select(PropertyMapper.ToResponse)
            .ToList();
    }

    public async Task<PropertyResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var property = await repository.GetByIdAsync(id, cancellationToken);

        return property is null
            ? null
            : PropertyMapper.ToResponse(property);
    }

    public async Task<PropertyResponse> CreateAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var property = PropertyMapper.ToDomain(request);
        var created = await repository.AddAsync(property, cancellationToken);

        return PropertyMapper.ToResponse(created);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var property = await repository.GetByIdAsync(id, cancellationToken);

        if (property is null)
            return false;

        PropertyMapper.MapToDomain(request, property);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var property = await repository.GetByIdAsync(id, cancellationToken);

        if (property is null)
            return false;

        repository.Delete(property);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
