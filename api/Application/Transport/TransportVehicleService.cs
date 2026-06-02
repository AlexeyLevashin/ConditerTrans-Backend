using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Transport.Requests;
using Contracts.Transport.Responses;
using Domain.Entities;
using FluentResults;

namespace Application.Transport;

public class TransportVehicleService(
    ITransportVehicleRepository transportVehicleRepository,
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ITransportVehicleService
{
    private const string CoordinatorOnlyError = "Доступно только логисту-координатору";

    public async Task<Result<List<VehicleBrandResponse>>> GetBrandsAsync(Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var brands = await transportVehicleRepository.GetBrandsAsync();
        return Result.Ok(brands.Select(b => new VehicleBrandResponse { Id = b.Id, Name = b.Name }).ToList());
    }

    public async Task<Result<List<VehicleModelResponse>>> GetModelsAsync(
        Guid userId,
        UserRole userRole,
        Guid? brandId = null)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var models = await transportVehicleRepository.GetModelsAsync(brandId);
        return Result.Ok(models.Select(m => new VehicleModelResponse
        {
            Id = m.Id,
            Name = m.Name,
            BrandId = m.BrandId,
            BrandName = m.Brand.Name
        }).ToList());
    }

    public async Task<Result<TransportVehicleListItemResponse>> CreateAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        CreateTransportVehicleRequest request)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var registration = request.RegistrationNumber.Trim();
        if (string.IsNullOrWhiteSpace(registration))
        {
            return Result.Fail("Укажите регистрационный номер");
        }

        if (request.Capacity <= 0)
        {
            return Result.Fail("Укажите вместимость больше нуля");
        }

        if (await transportVehicleRepository.RegistrationExistsAsync(companyId, registration))
        {
            return Result.Fail("ТС с таким номером уже зарегистрировано");
        }

        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee is null || employee.CompanyId != companyId)
        {
            return Result.Fail("Сотрудник не найден в вашей компании");
        }

        var driver = await userRepository.GetDriverByEmployeeIdAsync(employee.Id);
        if (driver is null)
        {
            return Result.Fail("Сотрудник не является водителем");
        }

        if (await transportVehicleRepository.GetModelByIdAsync(request.ModelId) is null)
        {
            return Result.Fail("Модель ТС не найдена");
        }

        var vehicle = new TransportVehicle
        {
            RegistrationNumber = registration,
            Capacity = request.Capacity,
            EmployeeId = employee.Id,
            ModelId = request.ModelId,
            CompanyId = companyId
        };

        await transportVehicleRepository.AddAsync(vehicle);
        await unitOfWork.SaveChangesAsync();

        var created = await transportVehicleRepository.GetByIdForCompanyAsync(vehicle.Id, companyId);
        return created is null
            ? Result.Fail("Не удалось создать ТС")
            : Result.Ok(MapListItem(created));
    }

    public async Task<Result<List<TransportVehicleListItemResponse>>> GetAvailableAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId,
        Guid? driverId = null)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(CoordinatorOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var vehicles = await transportVehicleRepository.GetAvailableForCompanyAsync(companyId, driverId);
        return Result.Ok(vehicles.Select(MapListItem).ToList());
    }

    public async Task<Result<List<FreeTransportReportRowResponse>>> GetFreeTransportReportAsync(
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

        var vehicles = await transportVehicleRepository.GetFreeForReportAsync(companyId);
        var rows = vehicles.Select(v =>
        {
            var driverName = FormatEmployeeName(v.Employee);
            var vehicleLabel = $"{v.Model.Brand.Name} {v.Model.Name}";
            var city = ExtractCity(v.Company.Address);

            return new FreeTransportReportRowResponse
            {
                Driver = driverName,
                Vehicle = vehicleLabel,
                LicensePlate = v.RegistrationNumber,
                City = city,
                AvailableSince = DateTime.UtcNow.ToString("dd.MM.yyyy, HH:mm")
            };
        }).ToList();

        return Result.Ok(rows);
    }

    private static TransportVehicleListItemResponse MapListItem(TransportVehicle vehicle)
    {
        var brand = vehicle.Model.Brand.Name;
        var model = vehicle.Model.Name;
        var driverName = FormatEmployeeName(vehicle.Employee);

        return new TransportVehicleListItemResponse
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Capacity = vehicle.Capacity,
            EmployeeId = vehicle.EmployeeId,
            DriverName = driverName,
            BrandName = brand,
            ModelName = model,
            DisplayName = $"{brand} {model} · {vehicle.RegistrationNumber}",
            IsAvailable = true
        };
    }

    private static string FormatEmployeeName(Employee employee) =>
        string.Join(
            ' ',
            new[] { employee.Surname, employee.Name, employee.Patronymic }
                .Where(part => !string.IsNullOrWhiteSpace(part)));

    private static string ExtractCity(string address)
    {
        var part = address.Split(',')[0]?.Trim();
        return string.IsNullOrWhiteSpace(part) ? address : part;
    }
}
