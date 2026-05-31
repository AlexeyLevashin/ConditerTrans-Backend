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
    IUnitOfWork unitOfWork) : ICargoService
{
    private const string CoordinatorOnlyError = "Доступно только логисту-координатору";
    private const string DriverOnlyError = "Доступно только водителю";

    public async Task<Result<CargoListResponse>> GetPendingForCoordinatorAsync(Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        await EnsurePendingCargosFromOrdersAsync();

        var cargos = await cargoRepository.GetByStatusAsync(CargoStatus.NotAssignedToLogisticCompany);
        return Result.Ok(cargos.ToListDto());
    }

    public async Task<Result<CargoListResponse>> GetActiveForCoordinatorAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var cargos = await cargoRepository.GetActiveByCompanyIdAsync(companyId);
        return Result.Ok(cargos.ToListDto());
    }

    public async Task<Result<CargoListResponse>> GetAllForCoordinatorAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        CargoStatus? status)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        await EnsurePendingCargosFromOrdersAsync();

        var cargos = await cargoRepository.GetAllForCoordinatorAsync(companyId);

        if (status.HasValue)
        {
            cargos = cargos.Where(cargo => cargo.Status == status.Value).ToList();
        }

        return Result.Ok(cargos.ToListDto());
    }

    public async Task<Result<CargoListResponse>> GetActiveForDriverAsync(Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Driver)
        {
            return Result.Fail(DriverOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var cargos = await cargoRepository.GetActiveByDriverIdAsync(userId);
        return Result.Ok(cargos.ToListDto());
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

        var cargo = await cargoRepository.GetByIdWithOrderAsync(cargoId);
        if (cargo is null || cargo.Status != CargoStatus.NotAssignedToLogisticCompany)
        {
            return Result.Fail("Груз не найден или уже обработан");
        }

        var assigned = await cargoRepository.AssignDriverAsync(cargoId, driver.Id, companyId);
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

    private async Task EnsurePendingCargosFromOrdersAsync()
    {
        var orders = await orderRepository.GetAwaitingShipmentWithoutCargoAsync();
        if (orders.Count == 0)
        {
            return;
        }

        foreach (var order in orders)
        {
            var (weight, volume) = CalculateWeightVolume(order);
            var now = DateTime.UtcNow;

            var cargo = new Domain.Entities.Cargo
            {
                LoadingDate = order.CreationDate,
                UnloadingDate = order.CreationDate.AddDays(7),
                DeliveryAddress = order.DeliveryAddress ?? "Адрес не указан",
                Weight = weight,
                Volume = volume,
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

    private static (decimal weight, decimal volume) CalculateWeightVolume(Order order)
    {
        decimal weight = 0;
        decimal volume = 0;

        foreach (var line in order.OrderLines.Where(line => line.Product is not null))
        {
            weight += line.QuantityOfUnits * (decimal)line.Product!.Quantity;
            volume += line.QuantityOfUnits;
        }

        return (weight, volume);
    }
}
