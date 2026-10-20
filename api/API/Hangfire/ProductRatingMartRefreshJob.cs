using DataAccess;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace API.Hangfire;

public sealed class ProductRatingMartRefreshJob(AppDbContext context)
{
    [DisableConcurrentExecution(300)]
    public Task RefreshAsync(CancellationToken cancellationToken) =>
        context.Database.ExecuteSqlRawAsync(
            "SELECT public.refresh_dispatcher_product_rating_daily()",
            cancellationToken);
}
