namespace AzureTaskManagement.Database;

public class TenantSchemaManager
{
    private readonly MyDbContext _dbContext;

    public TenantSchemaManager(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureTenantInfoAsync(string tenantId)
    {
        await _dbContext.CreateTenantIfNotExist();
    }
}