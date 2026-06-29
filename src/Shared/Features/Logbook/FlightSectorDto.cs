namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class FlightSectorDto
{
    public Guid? Id { get; set; }

    public int SectorOrder { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public Guid DepartureAirportId { get; set; }

    public string? DepartureAirportDisplay { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public Guid ArrivalAirportId { get; set; }

    public string? ArrivalAirportDisplay { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public DateTimeOffset BlockOff { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public DateTimeOffset BlockOn { get; set; }

    public DateTimeOffset? Takeoff { get; set; }

    public DateTimeOffset? Landing { get; set; }

    public int BlockTimeMinutes { get; set; }

    public int FlightTimeMinutes { get; set; }

    public int PicTimeMinutes { get; set; }

    public int SicTimeMinutes { get; set; }

    public int DualTimeMinutes { get; set; }

    public int NightTimeMinutes { get; set; }

    public int IfrTimeMinutes { get; set; }

    public bool IsIfr { get; set; }

    public bool IsNight { get; set; }

    public int DayTakeoffs { get; set; } = 1;

    public int NightTakeoffs { get; set; }

    public int DayLandings { get; set; } = 1;

    public int NightLandings { get; set; }

    [JsonIgnore, NotMapped]
    public string BlockTimeDisplay => FlightTimeFormatting.FormatMinutes(BlockTimeMinutes);

    [JsonIgnore, NotMapped]
    public string FlightTimeDisplay => FlightTimeFormatting.FormatMinutes(FlightTimeMinutes);

    [JsonIgnore, NotMapped]
    public string PicTimeDisplay => FlightTimeFormatting.FormatMinutes(PicTimeMinutes);

    [JsonIgnore, NotMapped]
    public string SicTimeDisplay => FlightTimeFormatting.FormatMinutes(SicTimeMinutes);

    [JsonIgnore, NotMapped]
    public string DualTimeDisplay => FlightTimeFormatting.FormatMinutes(DualTimeMinutes);

    [JsonIgnore, NotMapped]
    public string NightTimeDisplay => FlightTimeFormatting.FormatMinutes(NightTimeMinutes);

    [JsonIgnore, NotMapped]
    public string IfrTimeDisplay => FlightTimeFormatting.FormatMinutes(IfrTimeMinutes);
}
