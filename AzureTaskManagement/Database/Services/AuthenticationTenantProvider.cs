namespace AzureTaskManagement.Database.Services;

public class AuthenticationTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _tenant;

    public string TenantSchema
    {
        get
        {
            var claimsSchema = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "Tenant")?.Value;
            return claimsSchema ?? _tenant ?? string.Empty;
        }
    }

    public AuthenticationTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetTenantSchemaIfNotExist(string tenant)
    {
        _tenant = tenant;
    }
}