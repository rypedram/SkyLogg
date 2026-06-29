using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightReportsService
{
    [AutoInject] private AppDbContext dbContext = default!;

    public async Task<FlightStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var flights = await dbContext.FlightLogs
            .AsNoTracking()
            .Include(f => f.Aircraft)
            .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport)
            .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport)
            .Where(f => f.UserId == userId && !f.Deleted)
            .ToListAsync(cancellationToken);

        var sectors = flights.SelectMany(f => f.Sectors).ToList();
        var flightItems = flights.Select(f => new FlightStatisticItemDto
        {
            FlightLogId = f.Id,
            FlightDate = f.FlightDate,
            Route = string.Join(" → ", f.Sectors.OrderBy(s => s.SectorOrder).Select(s => s.DepartureAirport?.ICAO).Append(f.Sectors.OrderBy(s => s.SectorOrder).LastOrDefault()?.ArrivalAirport?.ICAO).Where(value => string.IsNullOrWhiteSpace(value) is false)),
            FlightMinutes = f.TotalFlightMinutes,
        }).ToList();

        return new FlightStatisticsDto
        {
            TotalFlightMinutes = flights.Sum(f => f.TotalFlightMinutes),
            TotalNightMinutes = flights.Sum(f => f.TotalNightMinutes),
            TotalIfrMinutes = flights.Sum(f => f.TotalIfrMinutes),
            FlightCount = flights.Count,
            AircraftCount = flights.Select(f => f.AircraftId).Distinct().Count(),
            AirportCount = sectors.SelectMany(s => new[] { s.DepartureAirportId, s.ArrivalAirportId }).Distinct().Count(),
            LongestFlight = flightItems.OrderByDescending(f => f.FlightMinutes).FirstOrDefault(),
            ShortestFlight = flightItems.Where(f => f.FlightMinutes > 0).OrderBy(f => f.FlightMinutes).FirstOrDefault(),
            MostUsedAircraft = flights.GroupBy(f => f.Aircraft?.Registration).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault(),
            MostVisitedAirport = sectors
                .SelectMany(s => new[] { s.DepartureAirport?.ICAO, s.ArrivalAirport?.ICAO })
                .Where(value => string.IsNullOrWhiteSpace(value) is false)
                .GroupBy(a => a)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault(),
            MonthlyTrends = flights
                .GroupBy(f => new { f.FlightDate.Year, f.FlightDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyFlightTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    FlightMinutes = g.Sum(f => f.TotalFlightMinutes),
                    FlightCount = g.Count(),
                })
                .ToList(),
        };
    }

    public async Task<List<AchievementStatusDto>> GetAchievementsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var stats = await GetAchievementMetricValuesAsync(userId, cancellationToken);
        var achievements = await dbContext.Achievements.AsNoTracking().OrderBy(a => a.Threshold).ToListAsync(cancellationToken);
        var unlocked = await dbContext.UserAchievements.AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToDictionaryAsync(a => a.AchievementId, cancellationToken);

        var statuses = new List<AchievementStatusDto>();
        foreach (var achievement in achievements)
        {
            var currentValue = stats.GetValueOrDefault(achievement.Metric ?? "", 0);
            if (currentValue >= achievement.Threshold && unlocked.ContainsKey(achievement.Id) is false)
            {
                var userAchievement = new UserAchievement { UserId = userId, AchievementId = achievement.Id };
                await dbContext.UserAchievements.AddAsync(userAchievement, cancellationToken);
                unlocked[achievement.Id] = userAchievement;
            }

            statuses.Add(new AchievementStatusDto
            {
                Code = achievement.Code,
                Name = achievement.Name,
                Description = achievement.Description,
                Threshold = achievement.Threshold,
                CurrentValue = currentValue,
                IsUnlocked = currentValue >= achievement.Threshold,
                UnlockedOn = unlocked.TryGetValue(achievement.Id, out var unlockedAchievement) ? unlockedAchievement.UnlockedOn : null,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return statuses;
    }

    private async Task<Dictionary<string, int>> GetAchievementMetricValuesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var flights = await dbContext.FlightLogs
            .AsNoTracking()
            .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport)
            .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport)
            .Where(f => f.UserId == userId && !f.Deleted)
            .ToListAsync(cancellationToken);

        var sectors = flights.SelectMany(f => f.Sectors).ToList();
        var internationalFlights = flights.Count(f => f.Sectors.Any(s =>
            string.IsNullOrWhiteSpace(s.DepartureAirport?.Country) is false &&
            string.IsNullOrWhiteSpace(s.ArrivalAirport?.Country) is false &&
            s.DepartureAirport.Country != s.ArrivalAirport.Country));

        return new()
        {
            ["TotalFlightHours"] = flights.Sum(f => f.TotalFlightMinutes) / 60,
            ["InternationalFlights"] = internationalFlights,
            ["DistinctAirports"] = sectors.SelectMany(s => new[] { s.DepartureAirportId, s.ArrivalAirportId }).Distinct().Count(),
            ["DistinctAircraft"] = flights.Select(f => f.AircraftId).Distinct().Count(),
            ["NightHours"] = flights.Sum(f => f.TotalNightMinutes) / 60,
            ["IfrHours"] = flights.Sum(f => f.TotalIfrMinutes) / 60,
        };
    }
}
