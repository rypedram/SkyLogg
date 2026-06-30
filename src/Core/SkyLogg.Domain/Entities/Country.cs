namespace SkyLogg.Domain.Entities;

public class Country : Common.Entity
{
    public string Name { get; set; } = string.Empty;

    public string IsoCode { get; set; } = string.Empty;
}
