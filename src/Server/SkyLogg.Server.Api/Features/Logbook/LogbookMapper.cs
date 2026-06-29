using Riok.Mapperly.Abstractions;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[Mapper]
public static partial class LogbookMapper
{
    public static partial IQueryable<AircraftDto> Project(this IQueryable<Aircraft> query);

    public static partial IQueryable<AircraftTypeDto> Project(this IQueryable<AircraftType> query);

    public static partial IQueryable<CountryDto> Project(this IQueryable<Country> query);

    public static partial IQueryable<GeoTimeZoneDto> Project(this IQueryable<GeoTimeZone> query);

    public static partial IQueryable<CrewMemberDto> Project(this IQueryable<CrewMember> query);

    public static partial FlightLogDto Map(this FlightLog source);

    public static partial AircraftDto Map(this Aircraft source);

    public static partial AircraftTypeDto Map(this AircraftType source);

    public static partial AirportDto Map(this Airport source);

    public static partial CountryDto Map(this Country source);

    public static partial GeoTimeZoneDto Map(this GeoTimeZone source);

    public static partial CityDto Map(this City source);

    public static partial CrewMemberDto Map(this CrewMember source);

    [MapperIgnoreSource(nameof(AircraftDto.AircraftTypeDisplay))]
    [MapperIgnoreTarget(nameof(Aircraft.AircraftType))]
    [MapperIgnoreTarget(nameof(Aircraft.FlightLogs))]
    [MapperIgnoreTarget(nameof(Aircraft.CreatedOn))]
    [MapperIgnoreTarget(nameof(Aircraft.UpdatedAt))]
    public static partial Aircraft Map(this AircraftDto source);

    [MapperIgnoreTarget(nameof(AircraftType.Aircraft))]
    [MapperIgnoreTarget(nameof(AircraftType.CreatedOn))]
    [MapperIgnoreTarget(nameof(AircraftType.UpdatedAt))]
    public static partial AircraftType Map(this AircraftTypeDto source);

    [MapperIgnoreTarget(nameof(CrewMember.User))]
    [MapperIgnoreTarget(nameof(CrewMember.FlightAssignments))]
    [MapperIgnoreTarget(nameof(CrewMember.Name))]
    public static partial CrewMember Map(this CrewMemberDto source);

    [MapperIgnoreTarget(nameof(FlightLog.User))]
    [MapperIgnoreTarget(nameof(FlightLog.Sectors))]
    [MapperIgnoreTarget(nameof(FlightLog.CrewAssignments))]
    [MapperIgnoreTarget(nameof(FlightLog.CreatedOn))]
    [MapperIgnoreTarget(nameof(FlightLog.Deleted))]
    public static partial FlightLog Map(this FlightLogDto source);
}
