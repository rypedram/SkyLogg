namespace SkyLogg.Domain.Entities;

public class FlightLog : Common.AuditableEntity
{
    public Guid UserId { get; set; }

    public Guid AircraftId { get; set; }

    public DateOnly FlightDate { get; set; }

    public string? Remarks { get; set; }

    public int TotalBlockMinutes { get; set; }

    public int TotalFlightMinutes { get; set; }

    public int TotalPicMinutes { get; set; }

    public int TotalSicMinutes { get; set; }

    public int TotalDualMinutes { get; set; }

    public int TotalNightMinutes { get; set; }

    public int TotalIfrMinutes { get; set; }

    public int TotalLandings { get; set; }

    public bool Deleted { get; set; }

    public List<FlightSector> Sectors { get; set; } = [];

    public List<FlightCrew> CrewAssignments { get; set; } = [];
}
