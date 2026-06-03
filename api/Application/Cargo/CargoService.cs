using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Cargo.Requests;
using Contracts.Cargo.Responses;
using Domain.Entities;
using FluentResults;

namespace Application.CargoHandling;

public class CargoService(
    ICargoRepository cargoRepository,
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    ITransportVehicleRepository transportVehicleRepository,
    IUnitOfWork unitOfWork) : ICargoService
{
    private const string CoordinatorOnlyError = "Доступно только логисту-координатору";
    private const string DriverOnlyError = "Доступно только водителю";

    public async Task<Result<CargoListResponse>> GetPendingForCoordinatorAsync(
        Guid userId,
        UserRole userRole,
        int page,
        int pageSize)
    {
        var access = await EnsureCoordinatorAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        await EnsurePendingCargosFromOrdersAsync();

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (cargos, totalCount) = await cargoRepository.GetByStatusPagedAsync(
            CargoStatus.NotAssignedToLogisticCompany,
            safePage,
            safePageSize);

        return Result.Ok(cargos.ToListDto(totalCount, safePage, safePageSize));
    }

    public async Task<Result<CargoListResponse>> GetActiveForCoordinatorAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        int page,
        int pageSize)
    {
        var access = await EnsureCoordinatorAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (cargos, totalCount) = await cargoRepository.GetActiveByCompanyIdPagedAsync(
            companyId,
            safePage,
            safePageSize);

        return Result.Ok(cargos.ToListDto(totalCount, safePage, safePageSize));
    }

    public async Task<Result<CargoListResponse>> GetAllForCoordinatorAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        CargoStatus? status,
        int page,
        int pageSize)
    {
        var access = await EnsureCoordinatorAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        await EnsurePendingCargosFromOrdersAsync();

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (cargos, totalCount) = await cargoRepository.GetAllForCoordinatorPagedAsync(
            companyId,
            status,
            safePage,
            safePageSize);

        return Result.Ok(cargos.ToListDto(totalCount, safePage, safePageSize));
    }

    public async Task<Result<CargoListResponse>> GetActiveForDriverAsync(
        Guid userId,
        UserRole userRole,
        int page,
        int pageSize)
    {
        if (userRole != UserRole.Driver)
        {
            return Result.Fail(DriverOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (cargos, totalCount) = await cargoRepository.GetActiveByDriverIdPagedAsync(
            userId,
            safePage,
            safePageSize);

        return Result.Ok(cargos.ToListDto(totalCount, safePage, safePageSize));
    }

    public async Task<Result> AssignDriverAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        Guid cargoId,
        AssignCargoDriverRequest request)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (request.DriverId == Guid.Empty)
        {
            return Result.Fail("Не выбран водитель");
        }

        if (request.TransportVehicleId == Guid.Empty)
        {
            return Result.Fail("Не выбрано транспортное средство");
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var driver = await userRepository.GetDriverByIdAndCompanyIdAsync(request.DriverId, companyId);
        if (driver is null)
        {
            return Result.Fail("Водитель не найден");
        }

        if (await cargoRepository.HasActiveCargoByDriverIdAsync(driver.Id))
        {
            return Result.Fail("Водитель уже занят на другом рейсе");
        }

        var vehicle = await transportVehicleRepository.GetByIdForCompanyAsync(
            request.TransportVehicleId,
            companyId);
        if (vehicle is null)
        {
            return Result.Fail("Транспортное средство не найдено");
        }

        if (vehicle.EmployeeId != driver.EmployeeId)
        {
            return Result.Fail("ТС не закреплено за выбранным водителем");
        }

        if (await transportVehicleRepository.IsBusyOnActiveTripAsync(vehicle.Id))
        {
            return Result.Fail("Транспортное средство уже занято на рейсе");
        }

        var cargo = await cargoRepository.GetByIdWithOrderAsync(cargoId);
        if (cargo is null || cargo.Status != CargoStatus.NotAssignedToLogisticCompany)
        {
            return Result.Fail("Груз не найден или уже обработан");
        }

        var assigned = await cargoRepository.AssignDriverAsync(
            cargoId,
            driver.Id,
            vehicle.Id,
            companyId);
        if (!assigned)
        {
            return Result.Fail("Не удалось назначить водителя");
        }

        if (cargo.Order is not null)
        {
            await orderRepository.MarkAsShippedAsync(cargo.Order.Id);
        }

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<CargoItemResponse>> GetByIdAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        Guid cargoId)
    {
        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var cargo = await cargoRepository.GetByIdWithOrderAsync(cargoId);
        if (cargo is null)
        {
            return Result.Fail("Груз не найден");
        }

        if (userRole == UserRole.Driver)
        {
            if (cargo.DriverId != userId)
            {
                return Result.Fail(DriverOnlyError);
            }
        }
        else if (userRole == UserRole.Coordinator)
        {
            if (cargo.LogisticCompanyId != companyId)
            {
                return Result.Fail(CoordinatorOnlyError);
            }
        }
        else
        {
            return Result.Fail("Недостаточно прав для просмотра груза");
        }

        return Result.Ok(cargo.ToItemDto());
    }

    private async Task<Result> EnsureCoordinatorAsync(Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        return Result.Ok();
    }

    private async Task EnsurePendingCargosFromOrdersAsync()
    {
        var orders = await orderRepository.GetAwaitingShipmentWithoutCargoAsync();
        if (orders.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var order in orders)
        {
            if (!order.ShipmentLengthM.HasValue ||
                !order.ShipmentWidthM.HasValue ||
                !order.ShipmentHeightM.HasValue ||
                !order.ShipmentWeightKg.HasValue)
            {
                continue;
            }

            var lengthM = order.ShipmentLengthM.Value;
            var widthM = order.ShipmentWidthM.Value;
            var heightM = order.ShipmentHeightM.Value;
            var weightKg = order.ShipmentWeightKg.Value;
            var volume = lengthM * widthM * heightM;
            var dimensionsText = $"{lengthM:0.##}×{widthM:0.##}×{heightM:0.##} м";

            var cargo = new Domain.Entities.Cargo
            {
                LoadingDate = now,
                UnloadingDate = now.AddDays(7),
                DeliveryAddress = order.DeliveryAddress ?? "Адрес не указан",
                Weight = weightKg,
                Volume = volume,
                Dimensions = dimensionsText,
                Status = CargoStatus.NotAssignedToLogisticCompany,
                Histories =
                [
                    new CargoChangeHistory
                    {
                        CargoStatus = CargoStatus.NotAssignedToLogisticCompany,
                        ChangeTime = now
                    }
                ]
            };

            await cargoRepository.AddAsync(cargo);
            await orderRepository.LinkCargoAsync(order.Id, cargo.Id);
        }

        await unitOfWork.SaveChangesAsync();
    }
}
