namespace SkyLogg.Domain.Entities;

public class AircraftType : Common.AuditableEntity
{
    public string Manufacturer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string? IcaoCode { get; set; }

    public string? IataCode { get; set; }

    public string? WakeCategory { get; set; }

    public int? EngineCount { get; set; }

    public string? Family { get; set; }
}
