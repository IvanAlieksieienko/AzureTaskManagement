using System.Security.Claims;
using AzureTaskManagement.Database.Services;

namespace AzureTaskManagement.Middleware;

public class TenantInitializer
{
    private RequestDelegate _next;
    private ITenantProvider _tenantProvider;
    
    public TenantInitializer(RequestDelegate next, ITenantProvider tenantProvider)
    {
        _next = next;
        _tenantProvider = tenantProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is null)
        {
            await _next(context);
            return;
        }
        
        if (context.User.Identity.IsAuthenticated)
        {
            var tenant = context.User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (tenant is null)
            {
                await _next(context);
                return;
            }
            _tenantProvider.SetTenantIfNotExist(tenant);
        }
        
        await _next(context);
    }
}