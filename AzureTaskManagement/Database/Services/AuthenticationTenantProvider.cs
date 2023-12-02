namespace AzureTaskManagement.Database.Services;

public class AuthenticationTenantProvider : ITenantProvider
{
    private Guid? _tenant;

    public Guid Tenant => _tenant ?? Guid.Empty;

    public void SetTenantIfNotExist(string tenant) => _tenant = Guid.Parse(tenant);

    public void ClearTenant() => _tenant = Guid.Empty;
}