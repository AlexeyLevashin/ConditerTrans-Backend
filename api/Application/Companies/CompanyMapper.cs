using Contracts.Company.Responses;
using Domain.Entities;

namespace Application.Companies;

internal static class CompanyMapper
{
    public static GetCompanyResponse ToResponse(Company company) => new()
    {
        Id = company.Id,
        Inn = company.Inn,
        Email = company.Email,
        Phone = company.Phone,
        Name = company.Name,
        Address = company.Address,
        Description = company.Description,
        CreatedAt = company.CreatedAt,
        CompanyType = company.CompanyType
    };
}
