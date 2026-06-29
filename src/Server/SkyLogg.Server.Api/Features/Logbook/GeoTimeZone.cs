using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class GeoTimeZone : IArchivable
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string? IanaId { get; set; }

    [Required, MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(10)]
    public string? UtcOffset { get; set; }

    public bool IsArchived { get; set; }

    public List<City> Cities { get; set; } = [];
}
