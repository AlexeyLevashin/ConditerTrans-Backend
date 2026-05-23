namespace Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string RegistrationNumber { get; set; }
    public double Capacity { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; }
    public Guid ModelId { get; set; }
    public virtual Model Model { get; set; }
    public Guid CompanyId { get; set; }
    public virtual Company Company { get; set; }
    public virtual ICollection<RouteSheet> RouteSheets { get; set; } = new List<RouteSheet>();
}