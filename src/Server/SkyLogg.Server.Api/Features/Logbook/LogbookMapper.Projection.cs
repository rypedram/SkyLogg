using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public static partial class LogbookMapper
{
    public static IQueryable<AirportDto> Project(this IQueryable<Airport> query)
    {
        return query.Select(a => new AirportDto
        {
            Id = a.Id,
            IATA = a.IATA,
            ICAO = a.ICAO,
            Name = a.Name,
            CityId = a.CityId,
            City = a.CityInfo!.Name,
            CountryId = a.CountryId,
            Country = a.Country ?? a.CountryInfo!.Name,
            TimeZoneDisplay = a.CityInfo!.TimeZone!.DisplayName,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            ElevationFt = a.ElevationFt,
            IsArchived = a.IsArchived,
        });
    }

    public static IQueryable<CityDto> Project(this IQueryable<City> query)
    {
        return query.Select(c => new CityDto
        {
            Id = c.Id,
            Name = c.Name,
            CountryId = c.CountryId,
            CountryName = c.CountryInfo!.Name,
            TimeZoneId = c.TimeZoneId,
            TimeZoneDisplay = c.TimeZone!.DisplayName,
            IsArchived = c.IsArchived,
        });
    }

    public static FlightLogDto MapFull(FlightLog source)
    {
        return new FlightLogDto
        {
            Id = source.Id,
            FlightDate = source.FlightDate,
            AircraftId = source.AircraftId,
            AircraftRegistration = source.Aircraft?.Registration,
            AircraftType = source.Aircraft?.Type,
            Remarks = source.Remarks,
            TotalBlockMinutes = source.TotalBlockMinutes,
            TotalFlightMinutes = source.TotalFlightMinutes,
            TotalPicMinutes = source.TotalPicMinutes,
            TotalSicMinutes = source.TotalSicMinutes,
            TotalDualMinutes = source.TotalDualMinutes,
            TotalNightMinutes = source.TotalNightMinutes,
            TotalIfrMinutes = source.TotalIfrMinutes,
            TotalLandings = source.TotalLandings,
            Version = source.Version,
            CreatedOn = source.CreatedOn,
            Sectors = source.Sectors.OrderBy(s => s.SectorOrder).Select(s => new FlightSectorDto
            {
                Id = s.Id,
                SectorOrder = s.SectorOrder,
                DepartureAirportId = s.DepartureAirportId,
                DepartureAirportDisplay = s.DepartureAirport is null ? null : $"{s.DepartureAirport.ICAO} — {s.DepartureAirport.Name}",
                ArrivalAirportId = s.ArrivalAirportId,
                ArrivalAirportDisplay = s.ArrivalAirport is null ? null : $"{s.ArrivalAirport.ICAO} — {s.ArrivalAirport.Name}",
                BlockOff = s.BlockOff,
                BlockOn = s.BlockOn,
                Takeoff = s.Takeoff,
                Landing = s.Landing,
                BlockTimeMinutes = s.BlockTimeMinutes,
                FlightTimeMinutes = s.FlightTimeMinutes,
                PicTimeMinutes = s.PicTimeMinutes,
                SicTimeMinutes = s.SicTimeMinutes,
                DualTimeMinutes = s.DualTimeMinutes,
                NightTimeMinutes = s.NightTimeMinutes,
                IfrTimeMinutes = s.IfrTimeMinutes,
                IsIfr = s.IsIfr,
                IsNight = s.IsNight,
                DayTakeoffs = s.DayTakeoffs,
                NightTakeoffs = s.NightTakeoffs,
                DayLandings = s.DayLandings,
                NightLandings = s.NightLandings,
            }).ToList(),
            Crew = source.CrewAssignments.Select(c => new FlightLogCrewDto
            {
                CrewMemberId = c.CrewMemberId,
                CrewMemberName = c.CrewMember == null ? null : (c.CrewMember.FirstName + " " + c.CrewMember.LastName).Trim(),
                RoleType = c.RoleType,
            }).ToList(),
        };
    }

    public static IQueryable<FlightLogDto> ProjectList(this IQueryable<FlightLog> query)
    {
        return query.Select(f => new FlightLogDto
        {
            Id = f.Id,
            FlightDate = f.FlightDate,
            AircraftId = f.AircraftId,
            AircraftRegistration = f.Aircraft!.Registration,
            AircraftType = f.Aircraft.Type,
            Remarks = f.Remarks,
            TotalBlockMinutes = f.TotalBlockMinutes,
            TotalFlightMinutes = f.TotalFlightMinutes,
            TotalPicMinutes = f.TotalPicMinutes,
            TotalSicMinutes = f.TotalSicMinutes,
            TotalDualMinutes = f.TotalDualMinutes,
            TotalNightMinutes = f.TotalNightMinutes,
            TotalIfrMinutes = f.TotalIfrMinutes,
            TotalLandings = f.TotalLandings,
            Version = f.Version,
            CreatedOn = f.CreatedOn,
            Sectors = f.Sectors.OrderBy(s => s.SectorOrder).Select(s => new FlightSectorDto
            {
                Id = s.Id,
                SectorOrder = s.SectorOrder,
                DepartureAirportId = s.DepartureAirportId,
                DepartureAirportDisplay = s.DepartureAirport!.ICAO,
                ArrivalAirportId = s.ArrivalAirportId,
                ArrivalAirportDisplay = s.ArrivalAirport!.ICAO,
                BlockOff = s.BlockOff,
                BlockOn = s.BlockOn,
                BlockTimeMinutes = s.BlockTimeMinutes,
                FlightTimeMinutes = s.FlightTimeMinutes,
                PicTimeMinutes = s.PicTimeMinutes,
                SicTimeMinutes = s.SicTimeMinutes,
                DualTimeMinutes = s.DualTimeMinutes,
                NightTimeMinutes = s.NightTimeMinutes,
                IfrTimeMinutes = s.IfrTimeMinutes,
                IsIfr = s.IsIfr,
                IsNight = s.IsNight,
            }).ToList(),
            Crew = f.CrewAssignments.Select(c => new FlightLogCrewDto
            {
                CrewMemberId = c.CrewMemberId,
                CrewMemberName = (c.CrewMember!.FirstName + " " + c.CrewMember.LastName).Trim(),
                RoleType = c.RoleType,
            }).ToList(),
        });
    }
}
