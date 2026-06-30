namespace SkyLogg.Domain.Entities;

public class Airport : Common.AuditableEntity
{
    public string Icao { get; set; } = string.Empty;

    public string? Iata { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid CityId { get; set; }

    public Guid? CountryId { get; set; }

    public string? Country { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int? ElevationFt { get; set; }

    public bool IsArchived { get; set; }
}
