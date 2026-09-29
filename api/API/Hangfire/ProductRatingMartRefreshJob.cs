using DataAccess;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace API.Hangfire;

public sealed class ProductRatingMartRefreshJob(AppDbContext context)
{
    [DisableConcurrentExecution(300)]
    public Task RefreshAsync(CancellationToken cancellationToken) =>
        context.Database.ExecuteSqlRawAsync(
            "REFRESH MATERIALIZED VIEW CONCURRENTLY dispatcher_product_rating_daily",
            cancellationToken);
}
