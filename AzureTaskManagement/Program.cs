using System.Security.Claims;
using AzureTaskManagement.Database;
using AzureTaskManagement.Database.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("credentials.json", false);
}

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantSchemaManager>();
builder.Services.AddScoped<ITenantProvider, AuthenticationTenantProvider>();

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false
    };

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            var tenantId = context.Properties.Items.ContainsKey("TenantId")
                ? context.Properties.Items["TenantId"]
                : "common";

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
                tenantProvider.SetTenantSchemaIfNotExist(tenantId);
                var schemaManager = ctx.HttpContext.RequestServices.GetService<TenantSchemaManager>();
                await schemaManager.EnsureSchemaForTenantAsync(tenantId);
            }
        },
    };
});


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Development"));
});

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();