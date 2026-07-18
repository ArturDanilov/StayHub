using StayHub.Contracts.Sources;
using StayHub.Domain.Models;

namespace StayHub.Mapping.Sources;

public static class SourceMapper
{
    public static Source ToDomain(CreateSourceRequest request)
    {
        return new Source
        {
            Name = request.Name.Trim(),
            SourceType = request.SourceType.Trim(),
            Url = string.IsNullOrWhiteSpace(request.Url)
                ? null
                : request.Url.Trim(),
            IsEnabled = request.IsEnabled,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static void MapToDomain(
        UpdateSourceRequest request,
        Source source)
    {
        source.Name = request.Name.Trim();
        source.SourceType = request.SourceType.Trim();
        source.Url = string.IsNullOrWhiteSpace(request.Url)
            ? null
            : request.Url.Trim();
        source.IsEnabled = request.IsEnabled;
    }

    public static SourceResponse ToResponse(Source source)
    {
        return new SourceResponse(
            source.Id,
            source.Name,
            source.SourceType,
            source.Url,
            source.IsEnabled,
            source.CreatedAtUtc);
    }
}
