using SkyLogg.Application.Common.Interfaces;

namespace SkyLogg.Infrastructure.Persistence;

/// <summary>
/// Placeholder unit of work. Replace with EF Core DbContext adapter in Server.Api integration.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);
}
