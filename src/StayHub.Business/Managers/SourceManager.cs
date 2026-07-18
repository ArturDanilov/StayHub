using StayHub.Business.Interfaces;
using StayHub.Contracts.Sources;
using StayHub.Mapping.Sources;

namespace StayHub.Business.Managers;

public sealed class SourceManager(ISourceRepository repository)
    : ISourceManager
{
    public async Task<IReadOnlyList<SourceResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var sources = await repository.GetAllAsync(cancellationToken);

        return sources
            .Select(SourceMapper.ToResponse)
            .ToList();
    }

    public async Task<SourceResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var source = await repository.GetByIdAsync(
            id,
            cancellationToken);

        return source is null
            ? null
            : SourceMapper.ToResponse(source);
    }

    public async Task<SourceResponse> CreateAsync(
        CreateSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = SourceMapper.ToDomain(request);

        var created = await repository.AddAsync(
            source,
            cancellationToken);

        return SourceMapper.ToResponse(created);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (source is null)
            return false;

        SourceMapper.MapToDomain(request, source);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var source = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (source is null)
            return false;

        repository.Delete(source);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
