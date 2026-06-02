using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface ITransportVehicleRepository
{
    Task<List<VehicleBrand>> GetBrandsAsync();
    Task<List<VehicleModel>> GetModelsAsync(Guid? brandId = null);
    Task<List<TransportVehicle>> GetAvailableForCompanyAsync(Guid companyId, Guid? driverUserId = null);
    Task<TransportVehicle?> GetByIdForCompanyAsync(Guid vehicleId, Guid companyId);
    Task<VehicleModel?> GetModelByIdAsync(Guid modelId);
    Task<bool> IsBusyOnActiveTripAsync(Guid vehicleId);
    Task<List<TransportVehicle>> GetFreeForReportAsync(Guid companyId);
    Task AddAsync(TransportVehicle vehicle);
    Task<bool> RegistrationExistsAsync(Guid companyId, string registrationNumber);
}
