using Kiwbi.Application.Common;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Application.Developers.RegisterDeveloper;

/// <summary>Creates a DeveloperCompany tenant, its DeveloperAdmin Identity account and links them atomically.</summary>
public class RegisterDeveloperUseCase
{
    private readonly IDeveloperCompanyRepository _developerCompanyRepository;
    private readonly IAccountProvisioningService _accountProvisioningService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterDeveloperUseCase(
        IDeveloperCompanyRepository developerCompanyRepository,
        IAccountProvisioningService accountProvisioningService,
        IUnitOfWork unitOfWork)
    {
        _developerCompanyRepository = developerCompanyRepository;
        _accountProvisioningService = accountProvisioningService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterDeveloperResult>> ExecuteAsync(
        RegisterDeveloperCommand command,
        CancellationToken cancellationToken = default)
    {
        DeveloperCompany company;

        try
        {
            company = DeveloperCompany.Create(command.CompanyName);
        }
        catch (DomainException ex)
        {
            return Result.Failure<RegisterDeveloperResult>(ex.Message);
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _developerCompanyRepository.AddAsync(company, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var accountResult = await _accountProvisioningService.CreateDeveloperAdminAccountAsync(
                command.Email, command.Password, company.Id, ct);

            if (accountResult.IsFailure)
            {
                return Result.Failure<RegisterDeveloperResult>(accountResult.Error!);
            }

            return Result.Success(new RegisterDeveloperResult(company.Id, accountResult.Value!));
        }, cancellationToken);
    }
}
