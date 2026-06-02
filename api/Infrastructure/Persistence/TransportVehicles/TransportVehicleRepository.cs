using Application.Common.Interfaces.Persistence.Repositories;
using Common.Enums;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.TransportVehicles;

public class TransportVehicleRepository(AppDbContext context) : ITransportVehicleRepository
{
    public Task<List<VehicleBrand>> GetBrandsAsync()
    {
        return context.VehicleBrands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public Task<List<VehicleModel>> GetModelsAsync(Guid? brandId = null)
    {
        var query = context.VehicleModels
            .AsNoTracking()
            .Include(m => m.Brand)
            .AsQueryable();

        if (brandId.HasValue)
        {
            query = query.Where(m => m.BrandId == brandId.Value);
        }

        return query.OrderBy(m => m.Brand.Name).ThenBy(m => m.Name).ToListAsync();
    }

    public async Task AddAsync(TransportVehicle vehicle)
    {
        await context.TransportVehicles.AddAsync(vehicle);
    }

    public Task<bool> RegistrationExistsAsync(Guid companyId, string registrationNumber)
    {
        var normalized = registrationNumber.Trim().ToUpperInvariant();
        return context.TransportVehicles.AnyAsync(v =>
            v.CompanyId == companyId
            && v.RegistrationNumber.ToUpper() == normalized);
    }

    public Task<List<TransportVehicle>> GetAvailableForCompanyAsync(Guid companyId, Guid? driverUserId = null)
    {
        var busyVehicleIds = context.Cargos
            .Where(c =>
                c.TransportVehicleId != null
                && (c.Status == CargoStatus.AwaitingTransportation
                    || c.Status == CargoStatus.PickedUpFromProduction))
            .Select(c => c.TransportVehicleId!.Value);

        var query = context.TransportVehicles
            .AsNoTracking()
            .Include(v => v.Model)
                .ThenInclude(m => m.Brand)
            .Include(v => v.Employee)
            .Where(v => v.CompanyId == companyId && !busyVehicleIds.Contains(v.Id));

        if (driverUserId.HasValue)
        {
            query = query.Where(v =>
                context.Users.Any(u =>
                    u.Id == driverUserId.Value
                    && u.EmployeeId == v.EmployeeId
                    && u.UserRole == UserRole.Driver));
        }

        return query
            .OrderBy(v => v.RegistrationNumber)
            .ToListAsync();
    }

    public Task<TransportVehicle?> GetByIdForCompanyAsync(Guid vehicleId, Guid companyId)
    {
        return context.TransportVehicles
            .Include(v => v.Model)
                .ThenInclude(m => m.Brand)
            .Include(v => v.Employee)
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.CompanyId == companyId);
    }

    public Task<bool> IsBusyOnActiveTripAsync(Guid vehicleId)
    {
        return context.Cargos.AnyAsync(c =>
            c.TransportVehicleId == vehicleId
            && (c.Status == CargoStatus.AwaitingTransportation
                || c.Status == CargoStatus.PickedUpFromProduction));
    }

    public Task<List<TransportVehicle>> GetFreeForReportAsync(Guid companyId)
    {
        var busyVehicleIds = context.Cargos
            .Where(c =>
                c.TransportVehicleId != null
                && (c.Status == CargoStatus.AwaitingTransportation
                    || c.Status == CargoStatus.PickedUpFromProduction))
            .Select(c => c.TransportVehicleId!.Value);

        return context.TransportVehicles
            .AsNoTracking()
            .Include(v => v.Model)
                .ThenInclude(m => m.Brand)
            .Include(v => v.Employee)
            .Include(v => v.Company)
            .Where(v => v.CompanyId == companyId && !busyVehicleIds.Contains(v.Id))
            .OrderBy(v => v.RegistrationNumber)
            .ToListAsync();
    }

    public Task<VehicleModel?> GetModelByIdAsync(Guid modelId)
    {
        return context.VehicleModels
            .Include(m => m.Brand)
            .FirstOrDefaultAsync(m => m.Id == modelId);
    }
}
