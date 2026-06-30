namespace SkyLogg.Domain.Entities;

public class City : Common.Entity
{
    public string Name { get; set; } = string.Empty;

    public Guid CountryId { get; set; }
}
