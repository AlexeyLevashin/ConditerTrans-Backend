namespace Application.Common.Interfaces.Services;

public interface IFileServiceClient
{
    Task<IReadOnlyDictionary<Guid, string>> GetUrlsByIdsAsync(
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default);
}
