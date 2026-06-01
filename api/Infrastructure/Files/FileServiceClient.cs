using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Services;
using Application.Files.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Files;

public class FileServiceClient(
    HttpClient httpClient,
    IOptions<FileServiceOptions> options,
    ILogger<FileServiceClient> logger) : IFileServiceClient
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetUrlsByIdsAsync(
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default)
    {
        var ids = fileIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var query = string.Join(
            "&",
            ids.Select(id => $"file_ids={Uri.EscapeDataString(id.ToString())}"));

        var requestUri = $"file-service/files?{query}";

        try
        {
            var files = await httpClient.GetFromJsonAsync<List<FileServiceFileDto>>(
                requestUri,
                cancellationToken);

            if (files is null || files.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            return files
                .Where(f => Guid.TryParse(f.Id, out _))
                .ToDictionary(f => Guid.Parse(f.Id), f => f.Url);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Не удалось получить URL файлов из file-service ({BaseUrl})",
                options.Value.BaseUrl);

            return new Dictionary<Guid, string>();
        }
    }

    private sealed class FileServiceFileDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
