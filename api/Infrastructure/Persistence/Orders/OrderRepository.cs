using Application.Common.Interfaces.Persistence.Repositories;
using Application.Orders;
using System.Data;
using Common.Enums;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Orders;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public Task<Guid?> GetDraftOrderIdByManagerIdAsync(Guid managerId)
    {
        return context.Orders
            .Where(o => o.Status == OrderStatus.Draft && o.ManagerId == managerId)
            .OrderByDescending(o => o.CreationDate)
            .ThenByDescending(o => o.Id)
            .Select(o => (Guid?)o.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid?> EnsureSingleDraftPerManagerAsync(Guid managerId)
    {
        var draftIds = await context.Orders
            .Where(o => o.ManagerId == managerId && o.Status == OrderStatus.Draft)
            .OrderByDescending(o => o.CreationDate)
            .ThenByDescending(o => o.Id)
            .Select(o => o.Id)
            .ToListAsync();

        if (draftIds.Count == 0)
        {
            return null;
        }

        var keepId = draftIds[0];
        var extraIds = draftIds.Skip(1).ToList();

        if (extraIds.Count > 0)
        {
            await context.OrderChangeHistories
                .Where(h => extraIds.Contains(h.OrderId))
                .ExecuteDeleteAsync();

            await context.OrderLines
                .Where(ol => extraIds.Contains(ol.OrderId))
                .ExecuteDeleteAsync();

            await context.Orders
                .Where(o => extraIds.Contains(o.Id))
                .ExecuteDeleteAsync();
        }

        return keepId;
    }

    public Task<Order?> GetDraftByManagerIdAsync(Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Draft && o.ManagerId == managerId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .FirstOrDefaultAsync();
    }

    public Task<bool> HasBlockingManagerOrderAsync(Guid managerId)
    {
        return context.Orders.AnyAsync(o =>
            o.ManagerId == managerId &&
            (o.Status == OrderStatus.PendingApproval || o.Status == OrderStatus.Rescheduled));
    }

    public async Task CreateDraftAsync(Order order, Guid productId, int quantityOfUnits)
    {
        order.Histories.Add(new OrderChangeHistory
        {
            OrderStatus = OrderStatus.Draft,
            ChangeTime = DateTime.UtcNow
        });

        order.OrderLines.Add(new OrderLine
        {
            ProductId = productId,
            QuantityOfUnits = quantityOfUnits
        });

        await context.Orders.AddAsync(order);
    }

    public async Task CreateDraftWithLinesAsync(
        Order order,
        IReadOnlyList<(Guid ProductId, int QuantityOfUnits)> lines)
    {
        order.Histories.Add(new OrderChangeHistory
        {
            OrderStatus = OrderStatus.Draft,
            ChangeTime = DateTime.UtcNow,
            Comment = "Создан повтором заказа"
        });

        foreach (var (productId, quantityOfUnits) in lines)
        {
            order.OrderLines.Add(new OrderLine
            {
                ProductId = productId,
                QuantityOfUnits = quantityOfUnits
            });
        }

        await context.Orders.AddAsync(order);
    }

    public async Task DeleteDraftsByManagerIdAsync(Guid managerId)
    {
        var draftIds = await context.Orders
            .Where(o => o.ManagerId == managerId && o.Status == OrderStatus.Draft)
            .Select(o => o.Id)
            .ToListAsync();

        if (draftIds.Count == 0)
        {
            return;
        }

        await context.OrderChangeHistories
            .Where(h => draftIds.Contains(h.OrderId))
            .ExecuteDeleteAsync();

        await context.OrderLines
            .Where(ol => draftIds.Contains(ol.OrderId))
            .ExecuteDeleteAsync();

        await context.Orders
            .Where(o => draftIds.Contains(o.Id))
            .ExecuteDeleteAsync();
    }

    public async Task<bool> UpsertOrderLineAsync(Guid orderId, Guid productId, int quantityOfUnits)
    {
        var isDraft = await context.Orders.AnyAsync(o =>
            o.Id == orderId && o.Status == OrderStatus.Draft);

        if (!isDraft)
        {
            return false;
        }

        var rowsUpdated = await context.OrderLines
            .Where(ol => ol.OrderId == orderId && ol.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ol => ol.QuantityOfUnits, quantityOfUnits));

        if (rowsUpdated > 0)
        {
            return true;
        }

        await context.OrderLines.AddAsync(new OrderLine
        {
            OrderId = orderId,
            ProductId = productId,
            QuantityOfUnits = quantityOfUnits
        });

        return true;
    }

    public Task<Guid?> GetDraftProductionCompanyIdAsync(Guid orderId)
    {
        return context.OrderLines
            .AsNoTracking()
            .Where(ol => ol.OrderId == orderId)
            .Join(
                context.Products,
                ol => ol.ProductId,
                p => p.Id,
                (_, p) => (Guid?)p.CompanyId)
            .FirstOrDefaultAsync();
    }

    public Task<bool> DraftHasOtherProductionCompanyAsync(Guid orderId, Guid productCompanyId)
    {
        return context.OrderLines
            .AsNoTracking()
            .Where(ol => ol.OrderId == orderId)
            .Join(
                context.Products,
                ol => ol.ProductId,
                p => p.Id,
                (_, p) => p.CompanyId)
            .AnyAsync(companyId => companyId != productCompanyId);
    }

    public async Task<bool> RemoveOrderLineAsync(Guid orderId, Guid productId, int quantityOfUnits)
    {
        var isDraft = await context.Orders.AnyAsync(o =>
            o.Id == orderId && o.Status == OrderStatus.Draft);

        if (!isDraft)
        {
            return false;
        }

        var line = await context.OrderLines
            .FirstOrDefaultAsync(ol => ol.OrderId == orderId && ol.ProductId == productId);

        if (line is null)
        {
            return false;
        }

        if (quantityOfUnits >= line.QuantityOfUnits)
        {
            context.OrderLines.Remove(line);
        }
        else
        {
            line.QuantityOfUnits -= quantityOfUnits;
        }

        return true;
    }

    public Task<List<Order>> GetAllByManagerIdAsync(Guid managerId)
    {
        return BuildManagerHistoryQuery(managerId)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public async Task<(List<Order> Items, int TotalCount)> GetHistoryByManagerIdPagedAsync(
        Guid managerId,
        int page,
        int pageSize)
    {
        var query = BuildManagerHistoryQuery(managerId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreationDate)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    private IQueryable<Order> BuildManagerHistoryQuery(Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.ManagerId == managerId && o.Status != OrderStatus.Draft)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product);
    }

    public Task<Order?> GetByIdAndManagerIdAsync(Guid orderId, Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.ManagerId == managerId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.Histories)
            .FirstOrDefaultAsync();
    }

    public Task<bool> ExistsDraftForManagerAsync(Guid orderId, Guid managerId)
    {
        return context.Orders.AnyAsync(o =>
            o.Id == orderId && o.ManagerId == managerId && o.Status == OrderStatus.Draft);
    }

    public Task<bool> HasOrderLinesAsync(Guid orderId)
    {
        return context.OrderLines.AnyAsync(ol => ol.OrderId == orderId);
    }

    public async Task SubmitDraftAsync(
        Guid orderId,
        Guid managerId,
        string productionAddress,
        string deliveryAddress,
        string paymentType,
        DateTime requestedDeliveryDate)
    {
        var maxOrderNumber = await context.Orders
            .Where(o => o.OrderNumber > 0)
            .MaxAsync(o => (int?)o.OrderNumber) ?? 0;
        var nextOrderNumber = maxOrderNumber + 1;
        var deliveryDate = requestedDeliveryDate.Date;

        await context.Orders
            .Where(o => o.Id == orderId
                        && o.ManagerId == managerId
                        && o.Status == OrderStatus.Draft)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.ProductionAddress, productionAddress)
                .SetProperty(o => o.DeliveryAddress, deliveryAddress)
                .SetProperty(o => o.PaymentType, paymentType)
                .SetProperty(o => o.OrderNumber, nextOrderNumber)
                .SetProperty(o => o.RequestedDeliveryDate, deliveryDate)
                .SetProperty(o => o.Status, OrderStatus.PendingApproval));

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = OrderStatus.PendingApproval,
            ChangeTime = DateTime.UtcNow
        });
    }

    public Task<List<Order>> GetAwaitingShipmentAsync()
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.AwaitingShipment)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public Task<Order?> GetByIdWithLinesAsync(Guid orderId)
    {
        return context.Orders
            .Where(o => o.Id == orderId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync();
    }

    public Task<List<Order>> GetAwaitingShipmentWithoutCargoAsync()
    {
        return context.Orders
            .Where(o => o.Status == OrderStatus.AwaitingShipment && o.CargoId == null)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public async Task LinkCargoAsync(Guid orderId, Guid cargoId)
    {
        await context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.CargoId, cargoId));
    }

    public async Task MarkAsShippedAsync(Guid orderId)
    {
        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.AwaitingShipment);

        if (order is null)
        {
            return;
        }

        await UpdateStatusAsync(orderId, OrderStatus.Shipped, null, null, null);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetForDispatcherPagedAsync(
        Guid productionCompanyId,
        string? search,
        OrderStatus? status,
        int page,
        int pageSize)
    {
        var query = FilterDispatcherOrdersQuery(productionCompanyId, search, status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreationDate)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<bool> HasDispatcherOrdersRequiringDeadlineConfirmationAsync(Guid productionCompanyId)
    {
        return BuildDispatcherOrdersQuery(productionCompanyId)
            .AnyAsync(o =>
                o.Status == OrderStatus.Confirmed &&
                o.CargoId == null &&
                o.DeadlineConfirmationPhase != DeadlineConfirmationPhase.None);
    }

    private IQueryable<Order> FilterDispatcherOrdersQuery(
        Guid productionCompanyId,
        string? search,
        OrderStatus? status)
    {
        var query = BuildDispatcherOrdersQuery(productionCompanyId)
            .Where(o => o.Status != OrderStatus.Draft);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                o.OrderNumber.ToString().Contains(term) ||
                (o.DeliveryAddress != null && o.DeliveryAddress.Contains(term)) ||
                (o.Manager != null &&
                 o.Manager.Employee != null &&
                 o.Manager.Employee.Company != null &&
                 o.Manager.Employee.Company.Name.Contains(term)));
        }

        return query;
    }

    public Task<Order?> GetByIdForDispatcherAsync(Guid orderId, Guid productionCompanyId)
    {
        return BuildDispatcherOrdersQuery(productionCompanyId)
            .Include(o => o.Histories)
            .Where(o => o.Id == orderId && o.Status != OrderStatus.Draft)
            .FirstOrDefaultAsync();
    }

    public Task<bool> BelongsToProductionCompanyAsync(Guid orderId, Guid productionCompanyId)
    {
        return BuildDispatcherOrdersQuery(productionCompanyId)
            .AnyAsync(o => o.Id == orderId);
    }

    public async Task<bool> MarkReadyForShipmentAsync(
        Guid orderId,
        Guid dispatcherId,
        DateTime shipmentDate,
        decimal lengthM,
        decimal widthM,
        decimal heightM,
        decimal weightKg,
        Guid cargoId,
        string? comment)
    {
        var order = await context.Orders.FirstOrDefaultAsync(o =>
            o.Id == orderId && o.Status == OrderStatus.Confirmed && o.CargoId == null);

        if (order is null)
        {
            return false;
        }

        order.Status = OrderStatus.AwaitingShipment;
        order.DispatcherId = dispatcherId;
        order.ShipmentLengthM = lengthM;
        order.ShipmentWidthM = widthM;
        order.ShipmentHeightM = heightM;
        order.ShipmentWeightKg = weightKg;
        order.CargoId = cargoId;
        order.DeadlineConfirmationPhase = DeadlineConfirmationPhase.None;
        order.DeadlineConfirmationRequestedAt = null;
        order.DeadlineConfirmationExpiresAt = null;

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = OrderStatus.AwaitingShipment,
            ChangeTime = DateTime.UtcNow,
            Comment = comment
        });

        return true;
    }

    public async Task UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        Guid? dispatcherId,
        string? productionAddress,
        string? comment,
        DateTime? proposedDeliveryDate = null,
        string? rescheduleReason = null,
        bool clearRescheduleProposal = false,
        Guid? rejectionReasonId = null)
    {
        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return;
        }

        order.Status = status;

        if (status != OrderStatus.Draft && order.OrderNumber <= 0)
        {
            var maxOrderNumber = await context.Orders
                .Where(o => o.OrderNumber > 0)
                .MaxAsync(o => (int?)o.OrderNumber) ?? 0;
            order.OrderNumber = maxOrderNumber + 1;
        }

        if (dispatcherId.HasValue)
        {
            order.DispatcherId = dispatcherId.Value;
        }

        if (productionAddress is not null)
        {
            order.ProductionAddress = productionAddress;
        }

        if (clearRescheduleProposal)
        {
            order.ProposedDeliveryDate = null;
            order.RescheduleReason = null;
        }
        else if (proposedDeliveryDate.HasValue)
        {
            order.ProposedDeliveryDate = proposedDeliveryDate;
            order.RescheduleReason = rescheduleReason;
        }

        if (status == OrderStatus.Rescheduled)
        {
            order.DeadlineConfirmationPhase = DeadlineConfirmationPhase.None;
            order.DeadlineConfirmationRequestedAt = null;
            order.DeadlineConfirmationExpiresAt = null;
        }

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = status,
            ChangeTime = DateTime.UtcNow,
            Comment = comment,
            RejectionReasonId = rejectionReasonId
        });

        if (status == OrderStatus.Rejected && rejectionReasonId.HasValue)
        {
            var companyId = await context.OrderLines
                .Where(line => line.OrderId == orderId)
                .Select(line => line.Product!.CompanyId)
                .FirstAsync();
            var rejectedAt = DateTime.UtcNow;
            var fact = await context.DispatcherRejectionFacts.FindAsync(orderId);
            if (fact is null)
            {
                await context.DispatcherRejectionFacts.AddAsync(new DispatcherRejectionFact
                {
                    OrderId = orderId,
                    CompanyId = companyId,
                    RejectedAt = rejectedAt,
                    RejectionReasonId = rejectionReasonId.Value
                });
            }
            else
            {
                fact.CompanyId = companyId;
                fact.RejectedAt = rejectedAt;
                fact.RejectionReasonId = rejectionReasonId.Value;
            }
        }
    }

    public Task<List<Order>> GetOrdersNeedingDeadlineWindowOpenAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var today = utcNow.Date;

        return context.Orders
            .Where(o =>
                o.Status == OrderStatus.Confirmed &&
                o.CargoId == null &&
                o.RequestedDeliveryDate != null &&
                o.DeadlineConfirmationPhase == DeadlineConfirmationPhase.None &&
                today >= o.RequestedDeliveryDate!.Value.Date.AddDays(-2))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Order>> GetOrdersWithExpiredDeadlineConfirmationAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return context.Orders
            .Where(o =>
                o.Status == OrderStatus.Confirmed &&
                o.CargoId == null &&
                o.DeadlineConfirmationExpiresAt != null &&
                o.DeadlineConfirmationExpiresAt <= utcNow &&
                o.DeadlineConfirmationPhase != DeadlineConfirmationPhase.None)
            .ToListAsync(cancellationToken);
    }

    public async Task OpenDeadlineConfirmationAsync(
        Guid orderId,
        DeadlineConfirmationPhase phase,
        DateTime requestedAt,
        DateTime expiresAt,
        string comment)
    {
        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return;
        }

        order.DeadlineConfirmationPhase = phase;
        order.DeadlineConfirmationRequestedAt = requestedAt;
        order.DeadlineConfirmationExpiresAt = expiresAt;

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = order.Status,
            ChangeTime = requestedAt,
            Comment = comment
        });
    }

    public async Task ClearDeadlineConfirmationAsync(Guid orderId)
    {
        await context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.DeadlineConfirmationPhase, DeadlineConfirmationPhase.None)
                .SetProperty(o => o.DeadlineConfirmationRequestedAt, (DateTime?)null)
                .SetProperty(o => o.DeadlineConfirmationExpiresAt, (DateTime?)null));
    }

    public async Task SetRequestedDeliveryDateAsync(Guid orderId, DateTime requestedDeliveryDate)
    {
        await context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.RequestedDeliveryDate, requestedDeliveryDate.Date));
    }

    public async Task<List<(DateTime Date, int RejectionCount)>> GetRejectionStatisticsAsync(
        Guid productionCompanyId,
        DateTime? dateFromUtc,
        DateTime? dateToUtcExclusive)
    {
        var query = context.DispatcherRejectionFacts.AsNoTracking()
            .Where(f => f.CompanyId == productionCompanyId);

        if (dateFromUtc.HasValue)
        {
            query = query.Where(f => f.RejectedAt >= dateFromUtc.Value);
        }

        if (dateToUtcExclusive.HasValue)
        {
            query = query.Where(f => f.RejectedAt < dateToUtcExclusive.Value);
        }

        var rows = await query
            .GroupBy(f => f.RejectedAt.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .OrderBy(item => item.Date)
            .ToListAsync();
        return rows.Select(item => (item.Date, item.Count)).ToList();
    }

    public async Task<List<(Guid OrderId, int OrderNumber, DateTime RejectedAt, string Reason)>> GetRejectionsForDayAsync(
        Guid productionCompanyId, DateTime dateFromUtc, DateTime dateToUtcExclusive)
    {
        var rows = await context.DispatcherRejectionFacts.AsNoTracking()
            .Where(f => f.CompanyId == productionCompanyId && f.RejectedAt >= dateFromUtc && f.RejectedAt < dateToUtcExclusive)
            .OrderByDescending(f => f.RejectedAt)
            .Select(f => new { f.OrderId, f.Order.OrderNumber, f.RejectedAt, Reason = f.RejectionReason.Name })
            .ToListAsync();
        return rows.Select(item => (item.OrderId, item.OrderNumber, item.RejectedAt, item.Reason)).ToList();
    }

    public async Task<List<(string ProductName, int OrderCount)>> GetProductRatingAsync(
        Guid productionCompanyId,
        DateTime? dateFromUtc,
        DateTime? dateToUtcExclusive)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT product_name, SUM(order_count)::integer AS order_count
                FROM dispatcher_product_rating_daily
                WHERE company_id = @company_id
                """;

            AddParameter("@company_id", DbType.Guid, productionCompanyId);
            if (dateFromUtc.HasValue)
            {
                command.CommandText += " AND order_date >= @date_from";
                AddParameter("@date_from", DbType.Date, DateOnly.FromDateTime(dateFromUtc.Value));
            }

            if (dateToUtcExclusive.HasValue)
            {
                command.CommandText += " AND order_date < @date_to";
                AddParameter("@date_to", DbType.Date, DateOnly.FromDateTime(dateToUtcExclusive.Value));
            }

            command.CommandText += " GROUP BY product_name ORDER BY order_count DESC, product_name";

            var result = new List<(string ProductName, int OrderCount)>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((reader.GetString(0), reader.GetInt32(1)));
            }

            return result;

            void AddParameter(string name, DbType type, object value)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.DbType = type;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public async Task<List<PartnerReliabilityOrderFact>> GetPartnerReliabilityFactsAsync(
        Guid purchasingCompanyId,
        Guid partnerCompanyId,
        PartnerAnalysisKind partnerKind,
        DateTime? dateFromUtc,
        DateTime? dateToUtcExclusive)
    {
        IQueryable<Order> ordersQuery = context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered)
            .Where(o =>
                o.Manager != null &&
                o.Manager.Employee != null &&
                o.Manager.Employee.CompanyId == purchasingCompanyId);

        ordersQuery = partnerKind switch
        {
            PartnerAnalysisKind.Production => ordersQuery.Where(o =>
                o.OrderLines.Count > 0 &&
                o.OrderLines.All(ol => ol.Product != null && ol.Product.CompanyId == partnerCompanyId)),
            PartnerAnalysisKind.Transport => ordersQuery.Where(o =>
                o.Cargo != null && o.Cargo.LogisticCompanyId == partnerCompanyId),
            _ => ordersQuery.Where(_ => false)
        };

        var orderRows = await ordersQuery
            .Select(o => new
            {
                o.Id,
                o.RequestedDeliveryDate,
                CargoUnloading = o.Cargo != null ? o.Cargo.UnloadingDate : (DateTime?)null
            })
            .ToListAsync();

        if (orderRows.Count == 0)
        {
            return [];
        }

        var orderIds = orderRows.Select(row => row.Id).ToList();

        var deliveredAtByOrder = await context.OrderChangeHistories
            .AsNoTracking()
            .Where(h => orderIds.Contains(h.OrderId) && h.OrderStatus == OrderStatus.Delivered)
            .GroupBy(h => h.OrderId)
            .Select(group => new
            {
                OrderId = group.Key,
                DeliveredAt = group.Max(item => item.ChangeTime)
            })
            .ToDictionaryAsync(item => item.OrderId, item => item.DeliveredAt);

        var rescheduledOrderIds = await context.OrderChangeHistories
            .AsNoTracking()
            .Where(h => orderIds.Contains(h.OrderId) && h.OrderStatus == OrderStatus.Rescheduled)
            .Select(h => h.OrderId)
            .Distinct()
            .ToListAsync();

        var rescheduledSet = rescheduledOrderIds.ToHashSet();
        var facts = new List<PartnerReliabilityOrderFact>();

        foreach (var row in orderRows)
        {
            if (!deliveredAtByOrder.TryGetValue(row.Id, out var deliveredAt))
            {
                continue;
            }

            if (dateFromUtc.HasValue && deliveredAt < dateFromUtc.Value)
            {
                continue;
            }

            if (dateToUtcExclusive.HasValue && deliveredAt >= dateToUtcExclusive.Value)
            {
                continue;
            }

            var actualDelivery = partnerKind == PartnerAnalysisKind.Transport && row.CargoUnloading.HasValue
                ? (deliveredAt > row.CargoUnloading.Value ? deliveredAt : row.CargoUnloading.Value)
                : deliveredAt;

            facts.Add(new PartnerReliabilityOrderFact(
                row.Id,
                row.RequestedDeliveryDate,
                actualDelivery,
                rescheduledSet.Contains(row.Id)));
        }

        return facts;
    }

    private IQueryable<Order> BuildDispatcherOrdersQuery(Guid productionCompanyId)
    {
        return context.Orders
            .AsNoTracking()
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.Manager!)
                .ThenInclude(m => m.Employee!)
                    .ThenInclude(e => e.Company)
            .Include(o => o.Cargo)
                .ThenInclude(c => c!.Driver)
                    .ThenInclude(d => d!.Employee)
            .Where(o =>
                o.OrderLines.Count > 0 &&
                o.OrderLines.All(ol => ol.Product != null && ol.Product.CompanyId == productionCompanyId));
    }
}
