using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Reports;

public sealed class RejectionReportCache(IDistributedCache cache)
{
    private static readonly DistributedCacheEntryOptions ReportEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public async Task<(string Version, T? Value)> GetAsync<T>(
        Guid companyId,
        string reportKey,
        CancellationToken cancellationToken = default)
    {
        var version = await GetVersionAsync(companyId, cancellationToken);
        var json = await cache.GetStringAsync(DataKey(companyId, version, reportKey), cancellationToken);
        return (version, json is null ? default : JsonSerializer.Deserialize<T>(json));
    }

    public async Task SetAsync<T>(
        Guid companyId,
        string version,
        string reportKey,
        T value,
        CancellationToken cancellationToken = default)
    {
        // A rejection may have been committed while this report was being built.
        if (!string.Equals(version, await GetVersionAsync(companyId, cancellationToken), StringComparison.Ordinal))
        {
            return;
        }

        await cache.SetStringAsync(
            DataKey(companyId, version, reportKey),
            JsonSerializer.Serialize(value),
            ReportEntryOptions,
            cancellationToken);
    }

    public async Task<string> InvalidateAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var version = Guid.NewGuid().ToString("N");
        await cache.SetStringAsync(VersionKey(companyId), version, cancellationToken);
        return version;
    }

    private async Task<string> GetVersionAsync(Guid companyId, CancellationToken cancellationToken) =>
        await cache.GetStringAsync(VersionKey(companyId), cancellationToken) ?? "initial";

    private static string VersionKey(Guid companyId) => $"dispatcher:rejections:{companyId:N}:version";

    private static string DataKey(Guid companyId, string version, string reportKey) =>
        $"dispatcher:rejections:{companyId:N}:{version}:{reportKey}";
}
