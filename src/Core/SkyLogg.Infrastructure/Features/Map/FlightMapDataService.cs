using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Application.Features.Map.Dtos;
using SkyLogg.Application.Features.Map.Services;

namespace SkyLogg.Infrastructure.Features.Map;

public sealed class FlightMapDataService : IFlightMapDataService
{
    private readonly IFlightLogRepository flightLogs;
    private readonly IAirportRepository airports;

    public FlightMapDataService(IFlightLogRepository flightLogs, IAirportRepository airports)
    {
        this.flightLogs = flightLogs;
        this.airports = airports;
    }

    public async Task<FlightMapDataDto> GetMapDataAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? aircraftId,
        Guid? airportId,
        CancellationToken cancellationToken = default)
    {
        var (flights, _) = await flightLogs.GetPagedAsync(userId, 1, int.MaxValue, null, cancellationToken);

        var filtered = flights.AsEnumerable();

        if (from.HasValue)
            filtered = filtered.Where(f => f.FlightDate >= from.Value);

        if (to.HasValue)
            filtered = filtered.Where(f => f.FlightDate <= to.Value);

        if (aircraftId.HasValue)
            filtered = filtered.Where(f => f.AircraftId == aircraftId.Value);

        if (airportId.HasValue)
        {
            filtered = filtered.Where(f => f.Sectors.Any(s =>
                s.DepartureAirportId == airportId.Value || s.ArrivalAirportId == airportId.Value));
        }

        var routes = new List<FlightMapRouteDto>();
        var airportVisits = new Dictionary<Guid, (int Count, DateOnly? LastVisited)>();

        foreach (var flight in filtered)
        {
            foreach (var sector in flight.Sectors)
            {
                var dep = await airports.GetByIdAsync(sector.DepartureAirportId, cancellationToken);
                var arr = await airports.GetByIdAsync(sector.ArrivalAirportId, cancellationToken);

                if (dep is null || arr is null)
                    continue;

                routes.Add(new FlightMapRouteDto
                {
                    FlightLogId = flight.Id,
                    SectorId = sector.Id,
                    DepartureIcao = dep.Icao,
                    ArrivalIcao = arr.Icao,
                    DepartureLatitude = dep.Latitude,
                    DepartureLongitude = dep.Longitude,
                    ArrivalLatitude = arr.Latitude,
                    ArrivalLongitude = arr.Longitude,
                    FlightDate = flight.FlightDate,
                    AircraftId = flight.AircraftId
                });

                TrackVisit(airportVisits, dep.Id, flight.FlightDate);
                TrackVisit(airportVisits, arr.Id, flight.FlightDate);
            }
        }

        var pins = new List<FlightMapAirportPinDto>();
        foreach (var (airportIdKey, stats) in airportVisits)
        {
            var airport = await airports.GetByIdAsync(airportIdKey, cancellationToken);
            if (airport is null)
                continue;

            pins.Add(new FlightMapAirportPinDto
            {
                AirportId = airport.Id,
                Icao = airport.Icao,
                Name = airport.Name,
                Latitude = airport.Latitude,
                Longitude = airport.Longitude,
                VisitCount = stats.Count,
                LastVisited = stats.LastVisited
            });
        }

        return new FlightMapDataDto
        {
            Routes = routes,
            Airports = pins
        };
    }

    private static void TrackVisit(Dictionary<Guid, (int Count, DateOnly? LastVisited)> visits, Guid airportId, DateOnly flightDate)
    {
        if (!visits.TryGetValue(airportId, out var current))
        {
            visits[airportId] = (1, flightDate);
            return;
        }

        visits[airportId] = (
            current.Count + 1,
            current.LastVisited is null || flightDate > current.LastVisited ? flightDate : current.LastVisited);
    }
}
