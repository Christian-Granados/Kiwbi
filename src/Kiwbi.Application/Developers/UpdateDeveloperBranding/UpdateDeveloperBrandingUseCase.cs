using Kiwbi.Application.Common;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Application.Developers.UpdateDeveloperBranding;

/// <summary>Updates the branding of the DeveloperCompany owned by the currently authenticated user.</summary>
public class UpdateDeveloperBrandingUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IDeveloperCompanyRepository _developerCompanyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeveloperBrandingUseCase(
        ICurrentUser currentUser,
        IDeveloperCompanyRepository developerCompanyRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _developerCompanyRepository = developerCompanyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateDeveloperBrandingCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var company = await _developerCompanyRepository.GetByIdAsync(developerCompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure("No se ha encontrado la promotora.");
        }

        Branding branding;

        try
        {
            branding = Branding.Create(command.PrimaryColor, command.SecondaryColor, command.LogoPath);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        company.UpdateBranding(branding);

        _developerCompanyRepository.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
