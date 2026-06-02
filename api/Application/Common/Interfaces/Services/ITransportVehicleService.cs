using Common.Enums;
using Contracts.Transport.Requests;
using Contracts.Transport.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface ITransportVehicleService
{
    Task<Result<List<VehicleBrandResponse>>> GetBrandsAsync(Guid userId, UserRole userRole);
    Task<Result<List<VehicleModelResponse>>> GetModelsAsync(Guid userId, UserRole userRole, Guid? brandId = null);

    Task<Result<List<TransportVehicleListItemResponse>>> GetAvailableAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        Guid? driverId = null);

    Task<Result<TransportVehicleListItemResponse>> CreateAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        CreateTransportVehicleRequest request);

    Task<Result<List<FreeTransportReportRowResponse>>> GetFreeTransportReportAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId);
}
