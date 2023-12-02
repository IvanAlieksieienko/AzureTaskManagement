namespace AzureTaskManagement.Database.Services;

public interface ITenantProvider
{

    string TenantSchema { get; }
    void SetTenantSchemaIfNotExist(string tenant);
}