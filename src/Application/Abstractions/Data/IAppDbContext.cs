using CleanCqrs.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanCqrs.Application.Abstractions.Data;

/// <summary>
/// The persistence abstraction handlers depend on. Implemented by Infrastructure.
/// </summary>
public interface IAppDbContext
{
    /// <summary>All products.</summary>
    DbSet<Product> Products { get; }

    /// <summary>Persists tracked changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts a database transaction.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
