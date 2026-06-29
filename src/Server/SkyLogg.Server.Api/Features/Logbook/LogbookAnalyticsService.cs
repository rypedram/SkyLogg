using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class LogbookAnalyticsService
{
    [AutoInject] private AppDbContext dbContext = default!;

    public async Task<CurrencyStatusDto> GetCurrencyStatusAsync(Guid userId, CancellationToken cancellationToken)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));

        var sectors = await dbContext.FlightSectors
            .AsNoTracking()
            .Include(s => s.FlightLog)
            .Where(s => s.FlightLog!.UserId == userId && !s.FlightLog.Deleted && s.FlightLog.FlightDate >= since)
            .ToListAsync(cancellationToken);

        var allLogs = await dbContext.FlightLogs
            .AsNoTracking()
            .Where(f => f.UserId == userId && !f.Deleted)
            .ToListAsync(cancellationToken);

        var passengerTakeoffs = sectors.Sum(s => s.DayTakeoffs + s.NightTakeoffs);
        var passengerLandings = sectors.Sum(s => s.DayLandings + s.NightLandings);
        var nightTakeoffs = sectors.Where(s => s.IsNight).Sum(s => s.NightTakeoffs);
        var nightLandings = sectors.Where(s => s.IsNight).Sum(s => s.NightLandings);

        return new CurrencyStatusDto
        {
            PassengerTakeoffs90Days = passengerTakeoffs,
            PassengerLandings90Days = passengerLandings,
            PassengerCurrencyMet = passengerTakeoffs >= 3 && passengerLandings >= 3,
            NightTakeoffs90Days = nightTakeoffs,
            NightLandings90Days = nightLandings,
            NightCurrencyMet = nightTakeoffs >= 3 && nightLandings >= 3,
            TotalBlockMinutes = allLogs.Sum(l => l.TotalBlockMinutes),
            TotalFlightMinutes = allLogs.Sum(l => l.TotalFlightMinutes),
            TotalPicMinutes = allLogs.Sum(l => l.TotalPicMinutes),
            TotalNightMinutes = allLogs.Sum(l => l.TotalNightMinutes),
            TotalIfrMinutes = allLogs.Sum(l => l.TotalIfrMinutes),
        };
    }

    public async Task<FlightSummaryDto> GetSummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var logs = await dbContext.FlightLogs
            .AsNoTracking()
            .Include(f => f.Aircraft)
            .Where(f => f.UserId == userId && !f.Deleted && f.FlightDate >= fromDate && f.FlightDate <= toDate)
            .ToListAsync(cancellationToken);

        var logIds = logs.Select(l => l.Id).ToList();

        var sectors = await dbContext.FlightSectors
            .AsNoTracking()
            .Where(s => logIds.Contains(s.FlightLogId))
            .ToListAsync(cancellationToken);

        var aircraftUsage = logs
            .GroupBy(l => new { l.AircraftId, l.Aircraft!.Registration })
            .Select(g => new AircraftUsageStatDto
            {
                AircraftId = g.Key.AircraftId,
                Registration = g.Key.Registration,
                BlockMinutes = g.Sum(l => l.TotalBlockMinutes),
                FlightCount = g.Count(),
            })
            .OrderByDescending(a => a.BlockMinutes)
            .ToList();

        var airportVisits = sectors
            .SelectMany(s => new[] { s.DepartureAirportId, s.ArrivalAirportId })
            .GroupBy(id => id)
            .Select(g => new { AirportId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        var airportIds = airportVisits.Select(a => a.AirportId).ToList();
        var airports = await dbContext.Airports
            .AsNoTracking()
            .Where(a => airportIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        return new FlightSummaryDto
        {
            TotalBlockMinutes = logs.Sum(l => l.TotalBlockMinutes),
            TotalFlightMinutes = logs.Sum(l => l.TotalFlightMinutes),
            TotalPicMinutes = logs.Sum(l => l.TotalPicMinutes),
            TotalNightMinutes = logs.Sum(l => l.TotalNightMinutes),
            TotalIfrMinutes = logs.Sum(l => l.TotalIfrMinutes),
            FlightCount = logs.Count,
            AircraftUsage = aircraftUsage,
            TopAirports = airportVisits.Select(v =>
            {
                airports.TryGetValue(v.AirportId, out var airport);
                return new AirportStatDto
                {
                    AirportId = v.AirportId,
                    ICAO = airport?.ICAO,
                    Name = airport?.Name,
                    VisitCount = v.Count,
                };
            }).ToList(),
        };
    }
}
