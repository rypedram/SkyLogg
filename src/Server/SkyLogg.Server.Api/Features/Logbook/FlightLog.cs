using SkyLogg.Server.Api.Features.Identity.Models;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class FlightLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public Guid AircraftId { get; set; }

    [ForeignKey(nameof(AircraftId))]
    public Aircraft? Aircraft { get; set; }

    public DateOnly FlightDate { get; set; }

    [MaxLength(2000)]
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

    public long Version { get; set; }

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<FlightSector> Sectors { get; set; } = [];

    public List<FlightLogCrew> CrewAssignments { get; set; } = [];
}
