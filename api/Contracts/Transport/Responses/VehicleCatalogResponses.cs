namespace Contracts.Transport.Responses;

public class VehicleBrandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class VehicleModelResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = null!;
}
