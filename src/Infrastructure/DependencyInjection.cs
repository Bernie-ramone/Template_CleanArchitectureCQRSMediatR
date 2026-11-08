using CleanCqrs.Application.Abstractions.Data;
using CleanCqrs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCqrs.Infrastructure;

/// <summary>
/// Registers Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Name of the connection string in configuration.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>
    /// Adds the SQL Server <see cref="AppDbContext"/> and exposes it as <see cref="IAppDbContext"/>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // The connection string is resolved lazily so test hosts can override configuration.
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IAppDbContext>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());

        return services;
    }

    /// <summary>
    /// Creates the database schema if it does not exist. Intended for local development only.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
