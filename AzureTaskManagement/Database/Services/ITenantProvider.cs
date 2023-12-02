namespace AzureTaskManagement.Database.Services;

public interface ITenantProvider
{

    Guid Tenant { get; }
    void SetTenantIfNotExist(string tenant);
    void ClearTenant();
}