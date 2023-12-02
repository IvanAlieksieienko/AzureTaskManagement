using System.Security.Claims;
using AzureTaskManagement.Database;
using AzureTaskManagement.Database.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace AzureTaskManagement.Extensions;

public static class OpenIdConnectExtensions
{
    public static void ConfigureEvents(this OpenIdConnectOptions options)
    {
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                var tenantId = context.Properties.Items.ContainsKey("TenantId")
                    ? context.Properties.Items["TenantId"]
                    : "common";

                tenantId = "a12b6bf2-0dc0-4ee5-bead-772006c8ca2a"; // for personal accounts
                context.ProtocolMessage.IssuerAddress =
                    context.ProtocolMessage.IssuerAddress.Replace("common", tenantId);

                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                var tenantId = ctx.SecurityToken.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;
                if (!string.IsNullOrEmpty(tenantId))
                {
                    var claimsIdentity = ctx.Principal.Identity as ClaimsIdentity;
                    claimsIdentity?.AddClaim(new Claim("Tenant", tenantId));
                    var tenantProvider = ctx.HttpContext.RequestServices.GetService<ITenantProvider>();
                    tenantProvider?.SetTenantIfNotExist(tenantId);
                    var schemaManager = ctx.HttpContext.RequestServices.GetService<TenantSchemaManager>();
                    await schemaManager.EnsureTenantInfoAsync(tenantId);
                }
            },
            OnSignedOutCallbackRedirect = ctx =>
            {
                var tenantProvider = ctx.HttpContext.RequestServices.GetService<ITenantProvider>();
                tenantProvider?.ClearTenant();
                return Task.CompletedTask;
            }
        };
    }
}