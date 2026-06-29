using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class AircraftType : IArchivable
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string? Manufacturer { get; set; }

    [Required, MaxLength(100)]
    public string? Model { get; set; }

    [Required, MaxLength(50)]
    public string? TypeCode { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? EngineType { get; set; }

    public bool IsArchived { get; set; }

    public long Version { get; set; }

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<Aircraft> Aircraft { get; set; } = [];
}
