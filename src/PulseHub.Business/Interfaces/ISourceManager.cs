using PulseHub.Contracts.Sources;

namespace PulseHub.Business.Interfaces;

public interface ISourceManager
{
    Task<IReadOnlyList<SourceResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SourceResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<SourceResponse> CreateAsync(
        CreateSourceRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        UpdateSourceRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
