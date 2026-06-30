namespace SkyLogg.Application.Features.Flights.Dtos;

public sealed class FlightSectorDto
{
    public Guid? Id { get; set; }

    public int SectorOrder { get; set; }

    public Guid DepartureAirportId { get; set; }

    public Guid ArrivalAirportId { get; set; }

    public DateTimeOffset BlockOff { get; set; }

    public DateTimeOffset BlockOn { get; set; }

    public DateTimeOffset? Takeoff { get; set; }

    public DateTimeOffset? Landing { get; set; }

    public int PicTimeMinutes { get; set; }

    public int SicTimeMinutes { get; set; }

    public int DualTimeMinutes { get; set; }

    public int NightTimeMinutes { get; set; }

    public int IfrTimeMinutes { get; set; }

    public bool IsIfr { get; set; }

    public bool IsNight { get; set; }

    public int DayLandings { get; set; }

    public int NightLandings { get; set; }
}

public sealed class FlightCrewAssignmentDto
{
    public Guid CrewMemberId { get; set; }

    public Domain.Enums.CrewRoleType RoleType { get; set; }
}

public class FlightLogWriteDto
{
    public Guid AircraftId { get; set; }

    public DateOnly FlightDate { get; set; }

    public string? Remarks { get; set; }

    public List<FlightSectorDto> Sectors { get; set; } = [];

    public List<FlightCrewAssignmentDto> Crew { get; set; } = [];
}

public sealed class FlightLogReadDto : FlightLogWriteDto
{
    public Guid Id { get; set; }

    public long Version { get; set; }

    public int TotalBlockMinutes { get; set; }

    public int TotalFlightMinutes { get; set; }

    public int TotalLandings { get; set; }
}
