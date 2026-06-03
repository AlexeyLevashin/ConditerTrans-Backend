using Common.Enums;

namespace Contracts.Cargo.Responses;

public class CargoListResponse
{
    public List<CargoItemResponse> Result { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CargoItemResponse
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public int? OrderNumber { get; set; }
    public DateTime LoadingDate { get; set; }
    public DateTime UnloadingDate { get; set; }
    public string DeliveryAddress { get; set; } = null!;
    public string? ProductionAddress { get; set; }
    public decimal Volume { get; set; }
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public CargoStatus Status { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid? TransportVehicleId { get; set; }
    public string? VehicleDisplayName { get; set; }
    public string? LicensePlate { get; set; }
    public decimal? OrderAmount { get; set; }
    public string? PaymentType { get; set; }
    public List<CargoOrderLineResponse> OrderLines { get; set; } = [];
}

public class CargoOrderLineResponse
{
    public string ProductName { get; set; } = null!;
    public string FormattedQuantity { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public int QuantityOfUnits { get; set; }
}
