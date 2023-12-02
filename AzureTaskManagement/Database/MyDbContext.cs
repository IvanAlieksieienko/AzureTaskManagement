using AzureTaskManagement.Database.Models;
using AzureTaskManagement.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace AzureTaskManagement.Database;

public class MyDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public DbSet<TaskModel> Tasks { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_tenantProvider.TenantSchema);
        modelBuilder.Entity<TaskModel>()
            .HasKey(b => b.Id);
    }

    public async Task<bool> SchemaExistsAsync(string schemaName)
    {
        var conn = Database.GetDbConnection();
        try
        {
            await conn.OpenAsync();

            using (var command = conn.CreateCommand())
            {
                command.CommandText =
                    "SELECT schema_name FROM information_schema.schemata WHERE schema_name = @schemaName";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@schemaName";
                parameter.Value = schemaName;
                command.Parameters.Add(parameter);

                var result = await command.ExecuteScalarAsync();
                return result != null;
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task CreateSchemaAsync(string schemaName)
    {
        var connection = Database.GetDbConnection();
        try
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"CREATE SCHEMA [{schemaName}]";
                // var parameter = command.CreateParameter();
                // parameter.ParameterName = "@schemaName";
                // parameter.Value = schemaName;
                // command.Parameters.Add(parameter);

                await command.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}