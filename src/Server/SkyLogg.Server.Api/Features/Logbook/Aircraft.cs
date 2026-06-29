namespace SkyLogg.Server.Api.Features.Logbook;

public partial class Aircraft
{
    public Guid Id { get; set; }

    [Required, MaxLength(20)]
    public string? Registration { get; set; }

    public Guid? AircraftTypeId { get; set; }

    [ForeignKey(nameof(AircraftTypeId))]
    public AircraftType? AircraftType { get; set; }

    [Required, MaxLength(50)]
    public string? Type { get; set; }

    [Required, MaxLength(100)]
    public string? Model { get; set; }

    public bool IsActive { get; set; } = true;

    public long Version { get; set; }

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<FlightLog> FlightLogs { get; set; } = [];
}
