using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("orders")]
public class Order
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("order_number")]
    public int OrderNumber { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }
    
    [Column("production_address")]
    public string? ProductionAddress { get; set; }

    [Column("delivery_address")]
    public string? DeliveryAddress { get; set; }

    [Column("payment_type")]
    public string? PaymentType { get; set; }

    /// <summary>Предложенная производством дата поставки при пересогласовании.</summary>
    [Column("proposed_delivery_date")]
    public DateTime? ProposedDeliveryDate { get; set; }

    /// <summary>Причина срыва сроков от производства (ожидает ответа менеджера).</summary>
    [Column("reschedule_reason")]
    public string? RescheduleReason { get; set; }

    [Column("shipment_length_m")]
    public decimal? ShipmentLengthM { get; set; }

    [Column("shipment_width_m")]
    public decimal? ShipmentWidthM { get; set; }

    [Column("shipment_height_m")]
    public decimal? ShipmentHeightM { get; set; }

    [Column("shipment_weight_kg")]
    public decimal? ShipmentWeightKg { get; set; }

    /// <summary>Согласованная дата доставки (для напоминания за 2 дня).</summary>
    [Column("requested_delivery_date")]
    public DateTime? RequestedDeliveryDate { get; set; }

    [Column("deadline_confirmation_phase")]
    public DeadlineConfirmationPhase DeadlineConfirmationPhase { get; set; }

    [Column("deadline_confirmation_requested_at")]
    public DateTime? DeadlineConfirmationRequestedAt { get; set; }

    [Column("deadline_confirmation_expires_at")]
    public DateTime? DeadlineConfirmationExpiresAt { get; set; }
    
    [Column("status")]
    public OrderStatus Status { get; set; }

    [Column("manager_id")]
    public Guid ManagerId { get; set; }

    [Column("dispatcher_id")]
    public Guid? DispatcherId { get; set; }

    [Column("cargo_id")]
    public Guid? CargoId { get; set; }

    public virtual User? Manager { get; set; }
    public virtual User? Dispatcher { get; set; }
    public virtual Cargo? Cargo { get; set; }

    public List<OrderLine> OrderLines { get; set; } = new();
    public List<OrderChangeHistory> Histories { get; set; } = new();
}