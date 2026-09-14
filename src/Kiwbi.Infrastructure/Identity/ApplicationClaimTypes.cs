namespace Kiwbi.Infrastructure.Identity;

/// <summary>Custom claim types issued to the auth cookie principal in addition to Identity's defaults.</summary>
public static class ApplicationClaimTypes
{
    public const string DeveloperCompanyId = "kiwbi:developer_company_id";
}
