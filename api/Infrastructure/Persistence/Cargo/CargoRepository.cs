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

    public Task<List<Domain.Entities.Cargo>> GetByStatusAsync(CargoStatus status)
    {
        return context.Cargos
            .AsNoTracking()
            .Where(c => c.Status == status)
            .Include(c => c.Order)
                .ThenInclude(o => o!.OrderLines)
                    .ThenInclude(ol => ol.Product)
            .Include(c => c.Driver)
                .ThenInclude(d => d!.Employee)
            .Include(c => c.TransportVehicle)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m.Brand)
            .OrderByDescending(c => c.LoadingDate)
            .ToListAsync();
    }

    public Task<List<Domain.Entities.Cargo>> GetActiveByCompanyIdAsync(Guid companyId)
    {
        return context.Cargos
            .AsNoTracking()
            .Where(c =>
                c.LogisticCompanyId == companyId
                && (c.Status == CargoStatus.AwaitingTransportation
                    || c.Status == CargoStatus.PickedUpFromProduction))
            .Include(c => c.Order)
                .ThenInclude(o => o!.OrderLines)
                    .ThenInclude(ol => ol.Product)
            .Include(c => c.Driver)
                .ThenInclude(d => d!.Employee)
            .Include(c => c.TransportVehicle)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m.Brand)
            .OrderByDescending(c => c.LoadingDate)
            .ToListAsync();
    }

    public Task<List<Domain.Entities.Cargo>> GetActiveByDriverIdAsync(Guid driverId)
    {
        return context.Cargos
            .AsNoTracking()
            .Where(c =>
                c.DriverId == driverId
                && (c.Status == CargoStatus.AwaitingTransportation
                    || c.Status == CargoStatus.PickedUpFromProduction))
            .Include(c => c.Order)
                .ThenInclude(o => o!.OrderLines)
                    .ThenInclude(ol => ol.Product)
            .Include(c => c.TransportVehicle)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m.Brand)
            .OrderByDescending(c => c.LoadingDate)
            .ToListAsync();
    }

    public Task<List<Domain.Entities.Cargo>> GetAllForCoordinatorAsync(Guid companyId)
    {
        return context.Cargos
            .AsNoTracking()
            .Where(c =>
                c.Status == CargoStatus.NotAssignedToLogisticCompany
                || c.LogisticCompanyId == companyId)
            .Include(c => c.Order)
                .ThenInclude(o => o!.OrderLines)
                    .ThenInclude(ol => ol.Product)
            .Include(c => c.Driver)
                .ThenInclude(d => d!.Employee)
            .Include(c => c.TransportVehicle)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m.Brand)
            .OrderByDescending(c => c.LoadingDate)
            .ToListAsync();
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
}
