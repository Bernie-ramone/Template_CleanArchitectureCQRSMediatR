using CleanCqrs.Infrastructure;
using CleanCqrs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCqrs.Api.IntegrationTests;

/// <summary>
/// Hosts the API in memory against a throwaway SQL Server database, created once per test class.
/// Uses LocalDB by default; set TEST_SQL_SERVER to a connection string without a database name to use another server.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string LocalDb = @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";

    private readonly string _connectionString =
        $"{Environment.GetEnvironmentVariable("TEST_SQL_SERVER") ?? LocalDb};Database=CleanCqrsTests_{Guid.NewGuid():N}";

    public async Task InitializeAsync() => await Services.EnsureDatabaseCreatedAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await using (var scope = Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
        }

        await DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting($"ConnectionStrings:{DependencyInjection.ConnectionStringName}", _connectionString);
    }
}
