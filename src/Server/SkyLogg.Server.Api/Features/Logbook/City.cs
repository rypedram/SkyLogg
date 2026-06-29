using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class City : IArchivable
{
    public Guid Id { get; set; }

    [Required, MaxLength(150)]
    public string? Name { get; set; }

    public Guid CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    public Country? CountryInfo { get; set; }

    public Guid TimeZoneId { get; set; }

    [ForeignKey(nameof(TimeZoneId))]
    public GeoTimeZone? TimeZone { get; set; }

    public bool IsArchived { get; set; }

    public List<Airport> Airports { get; set; } = [];
}
