namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class FlightLogDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [Display(Name = nameof(AppStrings.FlightDate))]
    public DateOnly FlightDate { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public Guid AircraftId { get; set; }

    public string? AircraftRegistration { get; set; }

    public string? AircraftType { get; set; }

    [MaxLength(2000)]
    public string? Remarks { get; set; }

    public int TotalBlockMinutes { get; set; }

    public int TotalFlightMinutes { get; set; }

    public int TotalPicMinutes { get; set; }

    public int TotalSicMinutes { get; set; }

    public int TotalDualMinutes { get; set; }

    public int TotalNightMinutes { get; set; }

    public int TotalIfrMinutes { get; set; }

    public int TotalLandings { get; set; }

    public long Version { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MinLength(1, ErrorMessage = nameof(AppStrings.FlightLogRequiresSector))]
    public List<FlightSectorDto> Sectors { get; set; } = [];

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MinLength(1, ErrorMessage = nameof(AppStrings.FlightLogRequiresCrew))]
    public List<FlightLogCrewDto> Crew { get; set; } = [];

    [JsonIgnore, NotMapped]
    public string TotalBlockTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalBlockMinutes);

    [JsonIgnore, NotMapped]
    public string TotalFlightTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalFlightMinutes);

    [JsonIgnore, NotMapped]
    public string TotalPicTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalPicMinutes);

    [JsonIgnore, NotMapped]
    public string TotalSicTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalSicMinutes);

    [JsonIgnore, NotMapped]
    public string TotalDualTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalDualMinutes);

    [JsonIgnore, NotMapped]
    public string TotalNightTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalNightMinutes);

    [JsonIgnore, NotMapped]
    public string TotalIfrTimeDisplay => FlightTimeFormatting.FormatMinutes(TotalIfrMinutes);

    [JsonIgnore, NotMapped]
    public string RouteSummary => string.Join(" → ", Sectors.Select(s =>
        s.DepartureAirportDisplay?.Split('—').FirstOrDefault()?.Trim() ?? "?"));
}
