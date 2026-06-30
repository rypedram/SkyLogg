namespace SkyLogg.Domain.Entities;

public class Aircraft : Common.AuditableEntity
{
    public string Registration { get; set; } = string.Empty;

    public Guid? AircraftTypeId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
}
