using Common.Enums;
using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface ICargoRepository
{
    Task AddAsync(Cargo cargo);
    Task<Cargo?> GetByIdWithOrderAsync(Guid cargoId);
    Task<List<Cargo>> GetByStatusAsync(CargoStatus status);
    Task<List<Cargo>> GetActiveByCompanyIdAsync(Guid companyId);
    Task<List<Cargo>> GetActiveByDriverIdAsync(Guid driverId);
    Task<List<Cargo>> GetAllForCoordinatorAsync(Guid companyId);
    Task<bool> HasActiveCargoByDriverIdAsync(Guid driverId);
    Task<bool> AssignDriverAsync(Guid cargoId, Guid driverId, Guid companyId);
    Task UpdateStatusAsync(Guid cargoId, CargoStatus status);
}
