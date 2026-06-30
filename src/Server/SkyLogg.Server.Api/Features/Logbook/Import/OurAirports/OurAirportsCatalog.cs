namespace SkyLogg.Server.Api.Features.Logbook.Import.OurAirports;

public sealed partial class OurAirportsCatalog
{
    private const string AirportsCsvUrl = "https://davidmegginson.github.io/ourairports-data/airports.csv";
    private const string CountriesCsvUrl = "https://davidmegginson.github.io/ourairports-data/countries.csv";
    private static readonly TimeSpan CacheMaxAge = TimeSpan.FromDays(7);

    [AutoInject] private IHttpClientFactory httpClientFactory = default!;
    [AutoInject] private IWebHostEnvironment webHostEnvironment = default!;
    [AutoInject] private ILogger<OurAirportsCatalog> logger = default!;

    private readonly SemaphoreSlim loadLock = new(1, 1);
    private volatile bool isLoaded;
    private Dictionary<string, OurAirportRecord> byIcao = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, OurAirportRecord> byIata = new(StringComparer.OrdinalIgnoreCase);

    public async Task WarmUpAsync(CancellationToken cancellationToken = default)
        => await EnsureLoadedAsync(cancellationToken);

    public async Task<OurAirportRecord?> LookupAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length is not (3 or 4))
            return null;

        await EnsureLoadedAsync(cancellationToken);

        return normalized.Length == 4
            ? byIcao.GetValueOrDefault(normalized)
            : byIata.GetValueOrDefault(normalized);
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (isLoaded)
            return;

        await loadLock.WaitAsync(cancellationToken);
        try
        {
            if (isLoaded)
                return;

            var countries = await LoadCountriesAsync(cancellationToken);
            var airportsCsv = await DownloadCsvAsync("airports.csv", AirportsCsvUrl, cancellationToken);
            BuildAirportIndexes(ParseAirports(airportsCsv, countries));

            isLoaded = true;
            logger.LogInformation("OurAirports catalog loaded with {IcaoCount} ICAO and {IataCount} IATA entries.", byIcao.Count, byIata.Count);
        }
        finally
        {
            loadLock.Release();
        }
    }

    private async Task<Dictionary<string, string>> LoadCountriesAsync(CancellationToken cancellationToken)
    {
        var countriesCsv = await DownloadCsvAsync("countries.csv", CountriesCsvUrl, cancellationToken);
        var lines = countriesCsv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var header = CsvLineParser.Parse(lines[0]);
        var codeIndex = Array.FindIndex(header, h => h.Equals("code", StringComparison.OrdinalIgnoreCase));
        var nameIndex = Array.FindIndex(header, h => h.Equals("name", StringComparison.OrdinalIgnoreCase));

        var countries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines.Skip(1))
        {
            var fields = CsvLineParser.Parse(line);
            if (codeIndex < 0 || nameIndex < 0 || fields.Length <= Math.Max(codeIndex, nameIndex))
                continue;

            var code = fields[codeIndex].Trim();
            var name = fields[nameIndex].Trim();
            if (code.Length > 0 && name.Length > 0)
                countries[code] = name;
        }

        return countries;
    }

    private static IEnumerable<OurAirportRecord> ParseAirports(string csv, IReadOnlyDictionary<string, string> countries)
    {
        var lines = csv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
            yield break;

        var header = CsvLineParser.Parse(lines[0]);
        var indexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Length; i++)
            indexes[header[i]] = i;

        string? Field(string[] fields, string name)
            => indexes.TryGetValue(name, out var index) && index < fields.Length ? fields[index].Trim() : null;

        foreach (var line in lines.Skip(1))
        {
            var fields = CsvLineParser.Parse(line);
            var type = Field(fields, "type");
            if (string.Equals(type, "closed", StringComparison.OrdinalIgnoreCase))
                continue;

            var name = Field(fields, "name");
            var isoCountry = Field(fields, "iso_country");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(isoCountry))
                continue;

            if (!double.TryParse(Field(fields, "latitude_deg"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var latitude))
                continue;

            if (!double.TryParse(Field(fields, "longitude_deg"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var longitude))
                continue;

            int? elevationFt = int.TryParse(Field(fields, "elevation_ft"), out var elevation) ? elevation : null;

            var ident = Field(fields, "ident");
            var gpsCode = Field(fields, "gps_code");
            var iata = Field(fields, "iata_code");
            var icao = FirstValidIcao(ident, gpsCode);
            if (icao is null)
                continue;

            var countryName = countries.TryGetValue(isoCountry, out var resolvedCountry)
                ? resolvedCountry
                : isoCountry;

            yield return new OurAirportRecord
            {
                Icao = icao,
                Iata = string.IsNullOrWhiteSpace(iata) ? null : iata.ToUpperInvariant(),
                Name = name,
                City = Field(fields, "municipality") ?? string.Empty,
                CountryIso2 = isoCountry.ToUpperInvariant(),
                CountryName = countryName,
                Latitude = latitude,
                Longitude = longitude,
                ElevationFt = elevationFt,
                TypeRank = GetTypeRank(type)
            };
        }
    }

    private void BuildAirportIndexes(IEnumerable<OurAirportRecord> airports)
    {
        var icaoIndex = new Dictionary<string, OurAirportRecord>(StringComparer.OrdinalIgnoreCase);
        var iataIndex = new Dictionary<string, OurAirportRecord>(StringComparer.OrdinalIgnoreCase);

        foreach (var airport in airports)
        {
            if (icaoIndex.TryGetValue(airport.Icao, out var existingIcao))
            {
                if (airport.TypeRank > existingIcao.TypeRank)
                    icaoIndex[airport.Icao] = airport;
            }
            else
            {
                icaoIndex[airport.Icao] = airport;
            }

            if (string.IsNullOrWhiteSpace(airport.Iata))
                continue;

            if (iataIndex.TryGetValue(airport.Iata, out var existingIata))
            {
                if (airport.TypeRank > existingIata.TypeRank)
                    iataIndex[airport.Iata] = airport;
            }
            else
            {
                iataIndex[airport.Iata] = airport;
            }
        }

        byIcao = icaoIndex;
        byIata = iataIndex;
    }

    private async Task<string> DownloadCsvAsync(string fileName, string url, CancellationToken cancellationToken)
    {
        var cacheDir = Path.Combine(webHostEnvironment.ContentRootPath, "App_Data", "ourairports");
        Directory.CreateDirectory(cacheDir);
        var cachePath = Path.Combine(cacheDir, fileName);

        if (File.Exists(cachePath) && DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath) < CacheMaxAge)
            return await File.ReadAllTextAsync(cachePath, cancellationToken);

        var client = httpClientFactory.CreateClient("OurAirports");
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        await File.WriteAllTextAsync(cachePath, content, cancellationToken);
        return content;
    }

    private static string? FirstValidIcao(string? ident, string? gpsCode)
    {
        foreach (var candidate in new[] { ident, gpsCode })
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var normalized = candidate.Trim().ToUpperInvariant();
            if (normalized.Length == 4 && normalized.All(char.IsLetterOrDigit))
                return normalized;
        }

        return null;
    }

    private static int GetTypeRank(string? type) => type?.ToLowerInvariant() switch
    {
        "large_airport" => 5,
        "medium_airport" => 4,
        "small_airport" => 3,
        "seaplane_base" => 2,
        "heliport" => 1,
        "balloonport" => 1,
        _ => 0
    };
}
