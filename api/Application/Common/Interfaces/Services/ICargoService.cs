using Common.Enums;
using Contracts.Cargo.Requests;
using Contracts.Cargo.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface ICargoService
{
    Task<Result<CargoListResponse>> GetPendingForCoordinatorAsync(Guid userId, UserRole userRole);
    Task<Result<CargoListResponse>> GetActiveForCoordinatorAsync(Guid userId, UserRole userRole, Guid companyId);
    Task<Result<CargoListResponse>> GetAllForCoordinatorAsync(Guid userId, UserRole userRole, Guid companyId, CargoStatus? status);
    Task<Result<CargoListResponse>> GetActiveForDriverAsync(Guid userId, UserRole userRole);
    Task<Result> AssignDriverAsync(Guid userId, UserRole userRole, Guid companyId, Guid cargoId, AssignCargoDriverRequest request);
    Task<Result<CargoItemResponse>> GetByIdAsync(Guid userId, UserRole userRole, Guid companyId, Guid cargoId);
}
