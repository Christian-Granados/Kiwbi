using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.Developers;

/// <summary>Repository for the DeveloperCompany aggregate, extending the generic repository with tenant-specific queries.</summary>
public interface IDeveloperCompanyRepository : IRepository<DeveloperCompany>
{
}
