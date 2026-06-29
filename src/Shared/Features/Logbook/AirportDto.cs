namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class AirportDto
{
    public Guid Id { get; set; }

    [MaxLength(3)]
    public string? IATA { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(4)]
    public string? ICAO { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(200)]
    public string? Name { get; set; }

    public Guid? CountryId { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public int? ElevationFt { get; set; }

    public bool IsActive { get; set; } = true;

    public string DisplayName => string.IsNullOrEmpty(IATA)
        ? $"{ICAO} — {Name}"
        : $"{ICAO}/{IATA} — {Name}";
}
