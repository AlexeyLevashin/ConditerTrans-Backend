using Common.Enums;
using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface ICargoRepository
{
    Task AddAsync(Cargo cargo);
    Task<Cargo?> GetByIdWithOrderAsync(Guid cargoId);

    Task<(List<Cargo> Items, int TotalCount)> GetByStatusPagedAsync(
        CargoStatus status,
        int page,
        int pageSize);

    Task<(List<Cargo> Items, int TotalCount)> GetActiveByCompanyIdPagedAsync(
        Guid companyId,
        int page,
        int pageSize);

    Task<(List<Cargo> Items, int TotalCount)> GetActiveByDriverIdPagedAsync(
        Guid driverId,
        int page,
        int pageSize);

    Task<(List<Cargo> Items, int TotalCount)> GetAllForCoordinatorPagedAsync(
        Guid companyId,
        CargoStatus? status,
        int page,
        int pageSize);

    Task<bool> HasActiveCargoByDriverIdAsync(Guid driverId);
    Task<bool> AssignDriverAsync(
        Guid cargoId,
        Guid driverId,
        Guid transportVehicleId,
        Guid companyId);
    Task UpdateStatusAsync(Guid cargoId, CargoStatus status);
}
