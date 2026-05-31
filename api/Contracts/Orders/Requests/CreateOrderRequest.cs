using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Contracts.Orders.Requests;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Id продукта обязательно")]
    public Guid ProductId { get; set; }

    [DefaultValue(1)]
    [Range(1, 10000, ErrorMessage = "Количество должно быть больше нуля")]
    public int QuantityOfUnits { get; set; } = 1;
}