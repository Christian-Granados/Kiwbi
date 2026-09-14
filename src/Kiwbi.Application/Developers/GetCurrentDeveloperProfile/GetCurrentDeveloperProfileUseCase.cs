using Kiwbi.Application.Common;
using Kiwbi.Domain.Developers;

namespace Kiwbi.Application.Developers.GetCurrentDeveloperProfile;

/// <summary>Resolves the profile of the DeveloperCompany owned by the currently authenticated user.</summary>
public class GetCurrentDeveloperProfileUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IDeveloperCompanyRepository _developerCompanyRepository;

    public GetCurrentDeveloperProfileUseCase(ICurrentUser currentUser, IDeveloperCompanyRepository developerCompanyRepository)
    {
        _currentUser = currentUser;
        _developerCompanyRepository = developerCompanyRepository;
    }

    public async Task<Result<DeveloperProfileDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<DeveloperProfileDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var company = await _developerCompanyRepository.GetByIdAsync(developerCompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure<DeveloperProfileDto>("No se ha encontrado la promotora.");
        }

        return Result.Success(ToDto(company));
    }

    internal static DeveloperProfileDto ToDto(DeveloperCompany company) => new(
        company.Id,
        company.Name,
        company.Branding.LogoPath,
        company.Branding.PrimaryColor.Value,
        company.Branding.SecondaryColor?.Value);
}
