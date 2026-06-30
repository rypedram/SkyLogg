namespace SkyLogg.Server.Api.Features.Logbook.Import;

public partial class AirportResolutionService : IAirportResolutionService
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IExternalAviationDataProvider externalProvider = default!;

    public async Task<(Airport Airport, bool WasCreated)> GetOrCreateAirportByCodeAsync(string airportCode, CancellationToken cancellationToken = default)
    {
        var normalized = airportCode.Trim().ToUpperInvariant();
        if (normalized.Length is not (3 or 4))
            throw new BadRequestException($"Invalid airport code: {airportCode}. Expected ICAO (4) or IATA (3) characters.");

        var existing = await dbContext.Airports
            .NotArchived()
            .FirstOrDefaultAsync(a =>
                (normalized.Length == 4 && a.ICAO == normalized) ||
                (normalized.Length == 3 && a.IATA == normalized), cancellationToken);

        if (existing is not null)
        {
            if (IsPlaceholderAirport(existing) is false)
                return (existing, false);

            var refreshed = await RefreshPlaceholderAsync(existing, cancellationToken);
            if (refreshed is not null)
                return (refreshed, false);
        }

        var external = await externalProvider.ResolveAirportAsync(normalized, cancellationToken)
            ?? throw new ResourceNotFoundException($"Airport '{normalized}' was not found in the OurAirports database.");

        if (existing is not null)
        {
            await ApplyExternalDataAsync(existing, external, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return (existing, false);
        }

        var country = await GetOrCreateCountryAsync(external.Country, external.CountryIso2, cancellationToken);
        var timeZone = await GetOrCreateTimeZoneAsync(external.TimeZoneIanaId ?? "UTC", cancellationToken);
        var city = await GetOrCreateCityAsync(external.City, country.Id, timeZone.Id, cancellationToken);

        var airport = new Airport
        {
            Id = Guid.NewGuid(),
            ICAO = external.Icao,
            IATA = external.Iata,
            Name = external.Name,
            CityId = city.Id,
            CountryId = country.Id,
            Country = country.Name,
            Latitude = external.Latitude,
            Longitude = external.Longitude,
            ElevationFt = external.ElevationFt,
            IsArchived = false
        };

        await dbContext.Airports.AddAsync(airport, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (airport, true);
    }

    private async Task<Airport?> RefreshPlaceholderAsync(Airport placeholder, CancellationToken cancellationToken)
    {
        var lookupCode = placeholder.ICAO ?? placeholder.IATA;
        if (string.IsNullOrWhiteSpace(lookupCode))
            return null;

        var external = await externalProvider.ResolveAirportAsync(lookupCode, cancellationToken);
        if (external is null)
            return null;

        await ApplyExternalDataAsync(placeholder, external, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return placeholder;
    }

    private async Task ApplyExternalDataAsync(Airport airport, ResolvedAirportInfo external, CancellationToken cancellationToken)
    {
        var country = await GetOrCreateCountryAsync(external.Country, external.CountryIso2, cancellationToken);
        var timeZone = await GetOrCreateTimeZoneAsync(external.TimeZoneIanaId ?? "UTC", cancellationToken);
        var city = await GetOrCreateCityAsync(external.City, country.Id, timeZone.Id, cancellationToken);

        airport.ICAO = external.Icao;
        airport.IATA = external.Iata;
        airport.Name = external.Name;
        airport.CityId = city.Id;
        airport.CountryId = country.Id;
        airport.Country = country.Name;
        airport.Latitude = external.Latitude;
        airport.Longitude = external.Longitude;
        airport.ElevationFt = external.ElevationFt;
    }

    private static bool IsPlaceholderAirport(Airport airport)
    {
        if (string.Equals(airport.Country, "Unknown", StringComparison.OrdinalIgnoreCase))
            return true;

        if (airport.ICAO is not null &&
            string.Equals(airport.Name, $"Airport {airport.ICAO}", StringComparison.OrdinalIgnoreCase))
            return true;

        return airport.Latitude == 0 && airport.Longitude == 0;
    }

    private async Task<Country> GetOrCreateCountryAsync(string name, string? iso2, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        var existing = await dbContext.Countries
            .NotArchived()
            .FirstOrDefaultAsync(c =>
                c.Name == normalizedName ||
                (iso2 != null && c.Iso2 == iso2), cancellationToken);

        if (existing is not null)
            return existing;

        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Iso2 = iso2 ?? normalizedName[..Math.Min(2, normalizedName.Length)].ToUpperInvariant(),
            Iso3 = iso2 is not null ? iso2 + "X" : normalizedName[..Math.Min(3, normalizedName.Length)].ToUpperInvariant()
        };

        await dbContext.Countries.AddAsync(country, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return country;
    }

    private async Task<SkyLogg.Server.Api.Features.Logbook.GeoTimeZone> GetOrCreateTimeZoneAsync(string ianaId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.GeoTimeZones
            .NotArchived()
            .FirstOrDefaultAsync(t => t.IanaId == ianaId, cancellationToken);

        if (existing is not null)
            return existing;

        var timeZone = new SkyLogg.Server.Api.Features.Logbook.GeoTimeZone
        {
            Id = Guid.NewGuid(),
            IanaId = ianaId,
            DisplayName = ianaId,
            UtcOffset = "+00:00"
        };

        await dbContext.GeoTimeZones.AddAsync(timeZone, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return timeZone;
    }

    private async Task<City> GetOrCreateCityAsync(string name, Guid countryId, Guid timeZoneId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        var existing = await dbContext.Cities
            .NotArchived()
            .FirstOrDefaultAsync(c => c.Name == normalizedName && c.CountryId == countryId, cancellationToken);

        if (existing is not null)
            return existing;

        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            CountryId = countryId,
            TimeZoneId = timeZoneId,
            IsArchived = false
        };

        await dbContext.Cities.AddAsync(city, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return city;
    }
}
