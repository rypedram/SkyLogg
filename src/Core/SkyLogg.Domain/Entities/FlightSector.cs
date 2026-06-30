namespace SkyLogg.Domain.Entities;

public class FlightSector : Common.Entity
{
    public Guid FlightLogId { get; set; }

    public int SectorOrder { get; set; }

    public Guid DepartureAirportId { get; set; }

    public Guid ArrivalAirportId { get; set; }

    public DateTimeOffset BlockOff { get; set; }

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

    public int DayTakeoffs { get; set; }

    public int NightTakeoffs { get; set; }

    public int DayLandings { get; set; }

    public int NightLandings { get; set; }
}
