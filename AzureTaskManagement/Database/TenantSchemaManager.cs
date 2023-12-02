namespace AzureTaskManagement.Database;

public class TenantSchemaManager
{
    private readonly MyDbContext _dbContext;

    public TenantSchemaManager(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureSchemaForTenantAsync(string tenantId)
    {
        if (await _dbContext.SchemaExistsAsync(tenantId))
        {
            return;
        }

        await _dbContext.CreateSchemaAsync(tenantId);
    }
}