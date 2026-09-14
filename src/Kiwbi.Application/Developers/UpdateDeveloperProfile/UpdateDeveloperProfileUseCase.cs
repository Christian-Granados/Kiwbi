using Kiwbi.Application.Common;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Application.Developers.UpdateDeveloperProfile;

/// <summary>Updates the name of the DeveloperCompany owned by the currently authenticated user.</summary>
public class UpdateDeveloperProfileUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IDeveloperCompanyRepository _developerCompanyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeveloperProfileUseCase(
        ICurrentUser currentUser,
        IDeveloperCompanyRepository developerCompanyRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _developerCompanyRepository = developerCompanyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateDeveloperProfileCommand command, CancellationToken cancellationToken = default)
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

        try
        {
            company.Rename(command.Name);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _developerCompanyRepository.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
