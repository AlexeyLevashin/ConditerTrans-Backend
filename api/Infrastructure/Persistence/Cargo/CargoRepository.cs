using Application.Common.Interfaces.Persistence.Repositories;
using Common.Enums;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Cargo;

public class CargoRepository(AppDbContext context) : ICargoRepository
{
    public async Task AddAsync(Domain.Entities.Cargo cargo)
    {
        await context.Cargos.AddAsync(cargo);
    }

    public Task<Domain.Entities.Cargo?> GetByIdWithOrderAsync(Guid cargoId)
    {
        return context.Cargos
            .Include(c => c.Order)
                .ThenInclude(o => o!.OrderLines)
                    .ThenInclude(ol => ol.Product)
            .Include(c => c.Driver)
                .ThenInclude(d => d!.Employee)
            .Include(c => c.TransportVehicle)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m.Brand)
            .FirstOrDefaultAsync(c => c.Id == cargoId);
    }

    public async Task<(List<Domain.Entities.Cargo> Items, int TotalCount)> GetByStatusPagedAsync(
        CargoStatus status,
        int page,
        int pageSize)
    {
        var query = ListQuery().Where(c => c.Status == status);
        return await PageAsync(query, page, pageSize);
    }

    public async Task<(List<Domain.Entities.Cargo> Items, int TotalCount)> GetActiveByCompanyIdPagedAsync(
        Guid companyId,
        int page,
        int pageSize)
    {
        var query = ListQuery().Where(c =>
            c.LogisticCompanyId == companyId
            && (c.Status == CargoStatus.AwaitingTransportation
                || c.Status == CargoStatus.PickedUpFromProduction));

        return await PageAsync(query, page, pageSize);
    }

    public async Task<(List<Domain.Entities.Cargo> Items, int TotalCount)> GetActiveByDriverIdPagedAsync(
        Guid driverId,
        int page,
        int pageSize)
    {
        var query = ListQuery().Where(c =>
            c.DriverId == driverId
            && (c.Status == CargoStatus.AwaitingTransportation
                || c.Status == CargoStatus.PickedUpFromProduction));

        return await PageAsync(query, page, pageSize);
    }

    public async Task<(List<Domain.Entities.Cargo> Items, int TotalCount)> GetAllForCoordinatorPagedAsync(
        Guid companyId,
        CargoStatus? status,
        int page,
        int pageSize)
    {
        var query = ListQuery().Where(c =>
            c.Status == CargoStatus.NotAssignedToLogisticCompany
            || c.LogisticCompanyId == companyId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await PageAsync(query, page, pageSize);
    }

    public Task<bool> HasActiveCargoByDriverIdAsync(Guid driverId)
    {
        return context.Cargos.AnyAsync(c =>
            c.DriverId == driverId
            && (c.Status == CargoStatus.AwaitingTransportation
                || c.Status == CargoStatus.PickedUpFromProduction));
    }

    public async Task<bool> AssignDriverAsync(
        Guid cargoId,
        Guid driverId,
        Guid transportVehicleId,
        Guid companyId)
    {
        var cargo = await context.Cargos.FirstOrDefaultAsync(c =>
            c.Id == cargoId && c.Status == CargoStatus.NotAssignedToLogisticCompany);

        if (cargo is null)
        {
            return false;
        }

        cargo.LogisticCompanyId = companyId;
        cargo.DriverId = driverId;
        cargo.TransportVehicleId = transportVehicleId;
        cargo.Status = CargoStatus.AwaitingTransportation;

        await context.CargoChangeHistories.AddAsync(new CargoChangeHistory
        {
            CargoId = cargoId,
            CargoStatus = CargoStatus.AwaitingTransportation,
            ChangeTime = DateTime.UtcNow
        });

        return true;
    }

    public async Task UpdateStatusAsync(Guid cargoId, CargoStatus status)
    {
        var cargo = await context.Cargos.FirstOrDefaultAsync(c => c.Id == cargoId);
        if (cargo is null)
        {
            return;
        }

        cargo.Status = status;

        await context.CargoChangeHistories.AddAsync(new CargoChangeHistory
        {
            CargoId = cargoId,
            CargoStatus = status,
            ChangeTime = DateTime.UtcNow
        });
    }

    private IQueryable<Domain.Entities.Cargo> ListQuery()
    {
        return context.Cargos
            .AsNoTracking()
            .Include(c => c.Order)
                .ThenInclude(o => o!.OrderLines)
                    .ThenInclude(ol => ol.Product)
            .Include(c => c.Driver)
                .ThenInclude(d => d!.Employee)
            .Include(c => c.TransportVehicle)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m.Brand);
    }

    private static async Task<(List<Domain.Entities.Cargo> Items, int TotalCount)> PageAsync(
        IQueryable<Domain.Entities.Cargo> query,
        int page,
        int pageSize)
    {
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.LoadingDate)
            .ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
