using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("orders")]
public class Order
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("order_number")]
    public int OrderNumber { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("manager_id")]
    public Guid ManagerId { get; set; }

    [Column("dispatcher_id")]
    public Guid? DispatcherId { get; set; }

    [Column("cargo_id")]
    public Guid? CargoId { get; set; }

    public virtual User Manager { get; set; }
    public virtual User? Dispatcher { get; set; }
}