using Common.Enums;

namespace Contracts.Company.Responses;

public class GetCompanyResponse
{
    public Guid Id { get; set; }
    public string Inn { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public CompanyType CompanyType { get; set; }
}
