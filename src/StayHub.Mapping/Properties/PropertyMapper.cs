using StayHub.Contracts.Properties;
using StayHub.Domain.Models;

namespace StayHub.Mapping.Properties;

public static class PropertyMapper
{
    public static Property ToDomain(CreatePropertyRequest request)
    {
        return new Property
        {
            Name = request.Name.Trim(),
            City = request.City.Trim(),
            CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static void MapToDomain(
        UpdatePropertyRequest request,
        Property property)
    {
        property.Name = request.Name.Trim();
        property.City = request.City.Trim();
        property.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
    }

    public static PropertyResponse ToResponse(Property property)
    {
        return new PropertyResponse(
            property.Id,
            property.Name,
            property.City,
            property.CountryCode,
            property.CreatedAtUtc);
    }
}
