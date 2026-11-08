using CleanCqrs.Application.Abstractions.Data;
using CleanCqrs.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanCqrs.Application.Tests;

/// <summary>
/// In-memory IAppDbContext so handler tests stay independent of Infrastructure.
/// </summary>
internal sealed class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public static TestAppDbContext Create() =>
        new(new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Product>();
}
