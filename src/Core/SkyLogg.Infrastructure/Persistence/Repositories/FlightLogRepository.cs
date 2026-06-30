using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Domain.Entities;

namespace SkyLogg.Infrastructure.Persistence.Repositories;

/// <summary>
/// In-memory scaffold repository. Replace with EF-backed implementation during Server.Api integration.
/// </summary>
public sealed class FlightLogRepository : IFlightLogRepository
{
    private static readonly List<FlightLog> Store = [];

    public Task<FlightLog?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var item = Store.FirstOrDefault(f => f.Id == id && f.UserId == userId && !f.Deleted);
        return Task.FromResult(item);
    }

    public Task<(IReadOnlyList<FlightLog> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = Store.Where(f => f.UserId == userId && !f.Deleted).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f => f.Remarks?.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
        }

        var list = query.OrderByDescending(f => f.FlightDate).ToList();
        var total = list.Count;
        var pageItems = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<FlightLog>, int)>((pageItems, total));
    }

    public void Add(FlightLog flightLog) => Store.Add(flightLog);

    public void Update(FlightLog flightLog)
    {
    }

    public void Remove(FlightLog flightLog) => Store.Remove(flightLog);
}
