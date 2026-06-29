namespace SkyLogg.Shared.Features.Logbook;

public partial class CurrencyStatusDto
{
    public bool PassengerCurrencyMet { get; set; }

    public int PassengerTakeoffs90Days { get; set; }

    public int PassengerLandings90Days { get; set; }

    public bool NightCurrencyMet { get; set; }

    public int NightTakeoffs90Days { get; set; }

    public int NightLandings90Days { get; set; }

    public int TotalBlockMinutes { get; set; }

    public int TotalFlightMinutes { get; set; }

    public int TotalNightMinutes { get; set; }

    public int TotalIfrMinutes { get; set; }

    public int TotalPicMinutes { get; set; }
}
