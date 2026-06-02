using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("transport_vehicles")]
public class TransportVehicle
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("registration_number")]
    public string RegistrationNumber { get; set; } = null!;

    /// <summary>Грузоподъёмность / вместимость, т.</summary>
    [Column("capacity")]
    public decimal Capacity { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("model_id")]
    public Guid ModelId { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    public Employee Employee { get; set; } = null!;
    public VehicleModel Model { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
