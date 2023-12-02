using System.Security.Claims;
using AzureTaskManagement.Database;
using AzureTaskManagement.Database.Services;
using AzureTaskManagement.Extensions;
using AzureTaskManagement.Middleware;
using AzureTaskManagement.Notifications;
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
builder.Services.AddScoped<TenantSchemaManager>();
builder.Services.AddSingleton<ITenantProvider, AuthenticationTenantProvider>();
builder.Services.AddSingleton<IServiceBusSender, ServiceBusSender>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false };
    options.ConfigureEvents();
});


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Development"));
});

builder.Services.AddHostedService<ServiceBusListener>();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<TenantInitializer>();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();