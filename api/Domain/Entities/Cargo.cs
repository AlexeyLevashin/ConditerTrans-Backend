using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("cargos")]
public class Cargo
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("loading_date")]
    public DateTime LoadingDate { get; set; }

    [Column("unloading_date")]
    public DateTime UnloadingDate { get; set; }

    [Column("delivery_address")]
    public string DeliveryAddress { get; set; } = null!;

    [Column("volume")]
    public decimal Volume { get; set; }

    [Column("weight")]
    public decimal Weight { get; set; }

    /// <summary>Габариты для отображения, например «1.2x0.8x0.5 м».</summary>
    [Column("dimensions")]
    public string? Dimensions { get; set; }

    [Column("status")]
    public CargoStatus Status { get; set; }

    [Column("logistic_company_id")]
    public Guid? LogisticCompanyId { get; set; }

    [Column("driver_id")]
    public Guid? DriverId { get; set; }

    public virtual Company? LogisticCompany { get; set; }
    public virtual User? Driver { get; set; }
    public virtual Order? Order { get; set; }
    public List<CargoChangeHistory> Histories { get; set; } = [];
}
