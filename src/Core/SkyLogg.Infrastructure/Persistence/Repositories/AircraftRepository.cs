using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Domain.Entities;

namespace SkyLogg.Infrastructure.Persistence.Repositories;

public sealed class AircraftRepository : IAircraftRepository
{
    private static readonly List<Aircraft> Store = [];

    public Task<Aircraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(a => a.Id == id));

    public Task<Aircraft?> GetByRegistrationAsync(string registration, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(a =>
            string.Equals(a.Registration, registration, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<Aircraft>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Aircraft>>(Store.ToList());

    public void Add(Aircraft aircraft) => Store.Add(aircraft);

    public void Update(Aircraft aircraft)
    {
    }

    public void Remove(Aircraft aircraft) => Store.Remove(aircraft);
}
