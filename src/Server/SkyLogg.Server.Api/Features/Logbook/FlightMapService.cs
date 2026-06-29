using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightMapService
{
    [AutoInject] private AppDbContext dbContext = default!;

    private readonly IFlightMapProvider mapProvider = new SvgFlightMapProvider();

    public async Task<FlightMapDto> GetMapDataAsync(Guid userId, DateOnly? fromDate, DateOnly? toDate, Guid? aircraftId, int? year, int? month, CancellationToken cancellationToken)
    {
        var query = dbContext.FlightLogs
            .AsNoTracking()
            .Include(f => f.Aircraft)
            .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport!).ThenInclude(a => a.CountryInfo)
            .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport!).ThenInclude(a => a.CountryInfo)
            .Where(f => f.UserId == userId && !f.Deleted);

        if (fromDate.HasValue)
            query = query.Where(f => f.FlightDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(f => f.FlightDate <= toDate.Value);

        if (aircraftId.HasValue)
            query = query.Where(f => f.AircraftId == aircraftId.Value);

        if (year is >= 1900 and <= 2100)
            query = query.Where(f => f.FlightDate.Year == year.Value);

        if (month is >= 1 and <= 12)
            query = query.Where(f => f.FlightDate.Month == month.Value);

        var flights = await query
            .OrderBy(f => f.FlightDate)
            .ToListAsync(cancellationToken);

        var routes = flights
            .SelectMany(f => f.Sectors.Select(s => CreateRoute(f, s)))
            .Where(r => r is not null)
            .Select(r => r!)
            .ToList();

        var airportPins = routes
            .SelectMany(r => new[]
            {
                new AirportPinSeed(r.DepartureAirport, r.DepartureLatitude, r.DepartureLongitude),
                new AirportPinSeed(r.ArrivalAirport, r.ArrivalLatitude, r.ArrivalLongitude),
            })
            .Where(a => string.IsNullOrWhiteSpace(a.Code) is false)
            .GroupBy(a => new { a.Code, a.Latitude, a.Longitude })
            .Select(g => new FlightMapAirportPinDto
            {
                ICAO = g.Key.Code,
                Name = g.Key.Code,
                Latitude = g.Key.Latitude,
                Longitude = g.Key.Longitude,
                VisitCount = g.Count(),
            })
            .OrderByDescending(a => a.VisitCount)
            .ToList();

        var countryStats = flights
            .SelectMany(f => f.Sectors.SelectMany(s => new[] { GetCountryName(s.DepartureAirport), GetCountryName(s.ArrivalAirport) }))
            .Where(country => string.IsNullOrWhiteSpace(country) is false)
            .GroupBy(country => country)
            .Select(g => new FlightMapCountryStatDto { Country = g.Key, VisitCount = g.Count() })
            .OrderByDescending(c => c.VisitCount)
            .ToList();

        return new FlightMapDto
        {
            ProviderKey = mapProvider.Key,
            FlightCount = flights.Count,
            TotalFlightMinutes = flights.Sum(f => f.TotalFlightMinutes),
            Routes = routes,
            Airports = airportPins,
            CountryStats = countryStats,
        };
    }

    private static FlightMapRouteDto? CreateRoute(FlightLog flight, FlightSector sector)
    {
        if (sector.DepartureAirport is null || sector.ArrivalAirport is null)
            return null;

        if (HasMapCoordinates(sector.DepartureAirport) is false || HasMapCoordinates(sector.ArrivalAirport) is false)
            return null;

        return new FlightMapRouteDto
        {
            FlightLogId = flight.Id,
            FlightDate = flight.FlightDate,
            AircraftRegistration = flight.Aircraft?.Registration,
            DepartureAirport = sector.DepartureAirport.ICAO,
            ArrivalAirport = sector.ArrivalAirport.ICAO,
            DepartureLatitude = sector.DepartureAirport.Latitude,
            DepartureLongitude = sector.DepartureAirport.Longitude,
            ArrivalLatitude = sector.ArrivalAirport.Latitude,
            ArrivalLongitude = sector.ArrivalAirport.Longitude,
            FlightTimeMinutes = sector.FlightTimeMinutes,
            GreatCirclePoints = CalculateGreatCircle(
                sector.DepartureAirport.Latitude,
                sector.DepartureAirport.Longitude,
                sector.ArrivalAirport.Latitude,
                sector.ArrivalAirport.Longitude),
        };
    }

    private static List<FlightMapPointDto> CalculateGreatCircle(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
    {
        const int segments = 32;
        var fromLat = ToRadians(fromLatitude);
        var fromLon = ToRadians(fromLongitude);
        var toLat = ToRadians(toLatitude);
        var toLon = ToRadians(toLongitude);
        var delta = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((toLat - fromLat) / 2), 2) +
                                            Math.Cos(fromLat) * Math.Cos(toLat) * Math.Pow(Math.Sin((toLon - fromLon) / 2), 2)));

        if (delta == 0)
            return [new() { Latitude = fromLatitude, Longitude = fromLongitude }];

        var points = new List<FlightMapPointDto>(segments + 1);
        for (var i = 0; i <= segments; i++)
        {
            var fraction = (double)i / segments;
            var a = Math.Sin((1 - fraction) * delta) / Math.Sin(delta);
            var b = Math.Sin(fraction * delta) / Math.Sin(delta);
            var x = a * Math.Cos(fromLat) * Math.Cos(fromLon) + b * Math.Cos(toLat) * Math.Cos(toLon);
            var y = a * Math.Cos(fromLat) * Math.Sin(fromLon) + b * Math.Cos(toLat) * Math.Sin(toLon);
            var z = a * Math.Sin(fromLat) + b * Math.Sin(toLat);
            var lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            var lon = Math.Atan2(y, x);

            points.Add(new FlightMapPointDto { Latitude = ToDegrees(lat), Longitude = ToDegrees(lon) });
        }

        return points;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private static double ToDegrees(double radians) => radians * 180 / Math.PI;

    private static bool HasMapCoordinates(Airport airport) =>
        Math.Abs(airport.Latitude) > 0.0001 || Math.Abs(airport.Longitude) > 0.0001;

    private static string? GetCountryName(Airport? airport) =>
        airport?.CountryInfo?.Name ?? airport?.Country;

    private sealed record AirportPinSeed(string? Code, double Latitude, double Longitude);
}
