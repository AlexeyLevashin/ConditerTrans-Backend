using System.ComponentModel.DataAnnotations;

namespace Contracts.RejectionReasons;

public class CreateRejectionReasonRequest
{
    [Required(ErrorMessage = "Название причины обязательно")]
    [StringLength(250, MinimumLength = 3, ErrorMessage = "Название причины должно содержать от 3 до 250 символов")]
    public string Name { get; set; } = null!;
}
