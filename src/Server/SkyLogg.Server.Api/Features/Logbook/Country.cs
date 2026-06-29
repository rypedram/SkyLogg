using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class Country : IArchivable
{
    public Guid Id { get; set; }

    [Required, MaxLength(2)]
    public string? Iso2 { get; set; }

    [Required, MaxLength(3)]
    public string? Iso3 { get; set; }

    [Required, MaxLength(100)]
    public string? Name { get; set; }

    public bool IsArchived { get; set; }

    public List<City> Cities { get; set; } = [];

    public List<Airport> Airports { get; set; } = [];
}
