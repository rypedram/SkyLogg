using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Domain.Entities;

namespace SkyLogg.Infrastructure.Persistence.Repositories;

public sealed class AirportRepository : IAirportRepository
{
    private static readonly List<Airport> Store = [];

    public Task<Airport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(a => a.Id == id));

    public Task<Airport?> GetByIcaoAsync(string icao, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(a =>
            string.Equals(a.Icao, icao, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<Airport>> SearchAsync(string query, int take, CancellationToken cancellationToken = default)
    {
        var normalized = query.Trim();
        var results = Store
            .Where(a => !a.IsArchived &&
                (a.Icao.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                 (a.Iata?.Contains(normalized, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 a.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase)))
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<Airport>>(results);
    }

    public void Add(Airport airport) => Store.Add(airport);

    public void Update(Airport airport)
    {
    }
}
