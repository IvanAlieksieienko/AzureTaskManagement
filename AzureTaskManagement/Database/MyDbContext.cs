using System.Security.Claims;
using AzureTaskManagement.Database.Models;
using AzureTaskManagement.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace AzureTaskManagement.Database;

public class MyDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<TenantModel> Tenants { get; set; }
    public DbSet<AttachmentModel> Attachments { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var tenantBuilder = modelBuilder.Entity<TenantModel>();
        tenantBuilder.HasKey(b => b.Id);

        var taskBuilder = modelBuilder.Entity<TaskModel>();
        taskBuilder.HasKey(b => b.Id);
        taskBuilder.HasOne(c => c.Tenant);

        var attachmentBuilder = modelBuilder.Entity<AttachmentModel>();
        attachmentBuilder.HasKey(a => a.FileName);
        attachmentBuilder.Ignore(a => a.FixedFileName);
        attachmentBuilder.HasOne(c => c.Task).WithMany(t => t.Attachments);

    }

    public async Task CreateTenantIfNotExist()
    {
        var currentTenantId = _tenantProvider.Tenant;
        var tenant = await Tenants.FindAsync(currentTenantId);
        if (tenant is not null)
        {
            return;
        }

        Tenants.Add(new TenantModel()
        {
            Id = currentTenantId
        });

        await SaveChangesAsync();
        await this.Database.CloseConnectionAsync();
    }
}