using SkyLogg.Domain.Entities;

namespace SkyLogg.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IFlightLogRepository
{
    Task<FlightLog?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FlightLog> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    void Add(FlightLog flightLog);

    void Update(FlightLog flightLog);

    void Remove(FlightLog flightLog);
}

public interface IAircraftRepository
{
    Task<Aircraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Aircraft?> GetByRegistrationAsync(string registration, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aircraft>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(Aircraft aircraft);

    void Update(Aircraft aircraft);

    void Remove(Aircraft aircraft);
}

public interface IAirportRepository
{
    Task<Airport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Airport?> GetByIcaoAsync(string icao, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Airport>> SearchAsync(string query, int take, CancellationToken cancellationToken = default);

    void Add(Airport airport);

    void Update(Airport airport);
}

public interface IImportSessionRepository
{
    Task<ImportSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    void Add(ImportSession session);

    void Update(ImportSession session);
}
