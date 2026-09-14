namespace Contracts.RejectionReasons;

public class RejectionReasonResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
}
