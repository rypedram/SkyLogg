namespace SkyLogg.Server.Api.Features.Logbook;

public partial class Airport
{
    public Guid Id { get; set; }

    [MaxLength(3)]
    public string? IATA { get; set; }

    [Required, MaxLength(4)]
    public string? ICAO { get; set; }

    [Required, MaxLength(200)]
    public string? Name { get; set; }

    public Guid? CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    public Country? CountryInfo { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int? ElevationFt { get; set; }

    public bool IsActive { get; set; } = true;
}
