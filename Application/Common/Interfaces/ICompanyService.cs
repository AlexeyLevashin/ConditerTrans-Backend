using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICompanyService
{
    Task<Company?> GetCompanyById(Guid companyId);
}