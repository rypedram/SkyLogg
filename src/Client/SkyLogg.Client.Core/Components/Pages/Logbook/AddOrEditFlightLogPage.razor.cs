using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class AddOrEditFlightLogPage
{
    [Parameter] public Guid? Id { get; set; }

    [AutoInject] private IFlightLogController flightLogController = default!;
    [AutoInject] private IAircraftController aircraftController = default!;
    [AutoInject] private ICrewMemberController crewMemberController = default!;

    private bool isSaving;
    private DateTimeOffset? flightDate;
    private FlightLogDto flightLog = new() { Sectors = [new FlightSectorDto()], Crew = [new FlightLogCrewDto { RoleType = CrewRoleType.PIC }] };
    private List<AircraftDto> aircraft = [];
    private List<BitDropdownItem<string>> aircraftItems = [];
    private string? aircraftValue;
    private List<CrewMemberDto> crewMembers = [];
    private List<BitDropdownItem<string>> crewItems = [];
    private List<BitDropdownItem<string>> roleItems = [];

    private List<DateTimeOffset?> sectorBlockOffDates = [DateTimeOffset.UtcNow];
    private List<DateTimeOffset?> sectorBlockOnDates = [DateTimeOffset.UtcNow];
    private List<string> sectorBlockOffTimes = ["08:00"];
    private List<string> sectorBlockOnTimes = ["10:00"];
    private List<string> sectorTakeoffTimes = [""];
    private List<string> sectorLandingTimes = [""];

    private int totalBlockMinutes;
    private int totalFlightMinutes;
    private int totalPicMinutes;
    private int totalSicMinutes;
    private int totalDualMinutes;
    private int totalNightMinutes;
    private int totalIfrMinutes;
    private int totalLandings;

    private string SelectedAircraftText =>
        aircraft.FirstOrDefault(a => a.Id == flightLog.AircraftId)?.Registration ??
        flightLog.AircraftRegistration ??
        "-";

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        roleItems =
        [
            new() { Text = "PIC", Value = CrewRoleType.PIC.ToString() },
            new() { Text = "SIC", Value = CrewRoleType.SIC.ToString() },
            new() { Text = "Crew", Value = CrewRoleType.Crew.ToString() },
        ];

        aircraft = await aircraftController.Get(CurrentCancellationToken);
        aircraftItems = aircraft.Select(a => new BitDropdownItem<string> { Value = a.Id.ToString(), Text = a.Registration }).ToList();
        aircraftValue = flightLog.AircraftId == Guid.Empty ? null : flightLog.AircraftId.ToString();

        crewMembers = await crewMemberController.Get(CurrentCancellationToken);
        crewItems = crewMembers.Select(c => new BitDropdownItem<string> { Value = c.Id.ToString(), Text = c.Name }).ToList();

        if (Id.HasValue)
        {
            flightLog = await flightLogController.Get(Id.Value, CurrentCancellationToken);
            flightDate = flightLog.FlightDate.ToDateTime(TimeOnly.MinValue);
            SyncSectorTimeFieldsFromModel();
        }
        else
        {
            flightDate = DateTimeOffset.UtcNow;
            flightLog.FlightDate = DateOnly.FromDateTime(flightDate.Value.DateTime);
        }

        RecalculateTimes();
    }

    private void SyncSectorTimeFieldsFromModel()
    {
        sectorBlockOffDates = flightLog.Sectors.Select(s => (DateTimeOffset?)s.BlockOff).ToList();
        sectorBlockOnDates = flightLog.Sectors.Select(s => (DateTimeOffset?)s.BlockOn).ToList();
        sectorBlockOffTimes = flightLog.Sectors.Select(s => s.BlockOff.ToString("HH:mm")).ToList();
        sectorBlockOnTimes = flightLog.Sectors.Select(s => s.BlockOn.ToString("HH:mm")).ToList();
        sectorTakeoffTimes = flightLog.Sectors.Select(s => s.Takeoff?.ToString("HH:mm") ?? "").ToList();
        sectorLandingTimes = flightLog.Sectors.Select(s => s.Landing?.ToString("HH:mm") ?? "").ToList();
    }

    private void ApplySectorTimesToModel()
    {
        for (var i = 0; i < flightLog.Sectors.Count; i++)
        {
            var sector = flightLog.Sectors[i];
            sector.BlockOff = CombineUtc(sectorBlockOffDates[i], sectorBlockOffTimes[i]);
            sector.BlockOn = CombineUtc(sectorBlockOnDates[i], sectorBlockOnTimes[i]);
            sector.Takeoff = ParseOptionalUtc(sectorBlockOffDates[i], sectorTakeoffTimes[i]);
            sector.Landing = ParseOptionalUtc(sectorBlockOnDates[i], sectorLandingTimes[i]);

            var (block, flight) = FlightTimeCalculatorClient.CalculateSectorTimes(sector.BlockOff, sector.BlockOn, sector.Takeoff, sector.Landing);
            sector.BlockTimeMinutes = block;
            sector.FlightTimeMinutes = flight;
            if (sector.PicTimeMinutes + sector.SicTimeMinutes + sector.DualTimeMinutes <= 0)
                sector.PicTimeMinutes = flight;

            if (sector.IsNight)
            {
                if (sector.NightTimeMinutes <= 0)
                    sector.NightTimeMinutes = flight;
            }
            else
            {
                sector.NightTimeMinutes = 0;
            }

            if (sector.IsIfr)
            {
                if (sector.IfrTimeMinutes <= 0)
                    sector.IfrTimeMinutes = flight;
            }
            else
            {
                sector.IfrTimeMinutes = 0;
            }
        }
    }

    private static DateTimeOffset CombineUtc(DateTimeOffset? date, string time)
    {
        if (TimeSpan.TryParse(time, out var ts) is false)
            ts = TimeSpan.Zero;

        var sourceDate = date ?? DateTimeOffset.UtcNow;
        return new DateTimeOffset(
            sourceDate.Year,
            sourceDate.Month,
            sourceDate.Day,
            ts.Hours,
            ts.Minutes,
            ts.Seconds,
            TimeSpan.Zero);
    }

    private static DateTimeOffset? ParseOptionalUtc(DateTimeOffset? date, string time)
    {
        if (string.IsNullOrWhiteSpace(time))
            return null;
        return CombineUtc(date, time);
    }

    private void RecalculateTimes()
    {
        ApplySectorTimesToModel();
        totalBlockMinutes = flightLog.Sectors.Sum(s => s.BlockTimeMinutes);
        totalFlightMinutes = flightLog.Sectors.Sum(s => s.FlightTimeMinutes);
        totalPicMinutes = flightLog.Sectors.Sum(s => s.PicTimeMinutes);
        totalSicMinutes = flightLog.Sectors.Sum(s => s.SicTimeMinutes);
        totalDualMinutes = flightLog.Sectors.Sum(s => s.DualTimeMinutes);
        totalNightMinutes = flightLog.Sectors.Sum(s => s.NightTimeMinutes);
        totalIfrMinutes = flightLog.Sectors.Sum(s => s.IfrTimeMinutes);
        totalLandings = flightLog.Sectors.Sum(s => s.DayLandings + s.NightLandings);
    }

    private void AddSector()
    {
        flightLog.Sectors.Add(new FlightSectorDto { DayTakeoffs = 1, DayLandings = 1 });
        sectorBlockOffDates.Add(DateTimeOffset.UtcNow);
        sectorBlockOnDates.Add(DateTimeOffset.UtcNow);
        sectorBlockOffTimes.Add("08:00");
        sectorBlockOnTimes.Add("10:00");
        sectorTakeoffTimes.Add("");
        sectorLandingTimes.Add("");
        RecalculateTimes();
    }

    private void RemoveSector(int index)
    {
        flightLog.Sectors.RemoveAt(index);
        sectorBlockOffDates.RemoveAt(index);
        sectorBlockOnDates.RemoveAt(index);
        sectorBlockOffTimes.RemoveAt(index);
        sectorBlockOnTimes.RemoveAt(index);
        sectorTakeoffTimes.RemoveAt(index);
        sectorLandingTimes.RemoveAt(index);
        RecalculateTimes();
    }

    private void AddCrew() => flightLog.Crew.Add(new FlightLogCrewDto { RoleType = CrewRoleType.SIC });

    private void RemoveCrew(int index) => flightLog.Crew.RemoveAt(index);

    private void OnAircraftChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
            flightLog.AircraftId = id;
        aircraftValue = value;
    }

    private void OnCrewChanged(int index, string? value)
    {
        if (Guid.TryParse(value, out var id))
            flightLog.Crew[index].CrewMemberId = id;
    }

    private void OnRoleChanged(int index, string? value)
    {
        if (Enum.TryParse<CrewRoleType>(value, out var role))
            flightLog.Crew[index].RoleType = role;
    }

    private void ApplyFlightDateToModel()
    {
        if (flightDate.HasValue)
            flightLog.FlightDate = DateOnly.FromDateTime(flightDate.Value.Date);
    }

    private async Task Save()
    {
        isSaving = true;
        try
        {
            ApplyFlightDateToModel();
            RecalculateTimes();
            flightLog.TotalBlockMinutes = totalBlockMinutes;
            flightLog.TotalFlightMinutes = totalFlightMinutes;
            flightLog.TotalPicMinutes = totalPicMinutes;
            flightLog.TotalSicMinutes = totalSicMinutes;
            flightLog.TotalDualMinutes = totalDualMinutes;
            flightLog.TotalNightMinutes = totalNightMinutes;
            flightLog.TotalIfrMinutes = totalIfrMinutes;
            flightLog.TotalLandings = totalLandings;

            if (Id.HasValue)
                await flightLogController.Update(flightLog, CurrentCancellationToken);
            else
                await flightLogController.Create(flightLog, CurrentCancellationToken);

            NavigationManager.NavigateTo(PageUrls.FlightLogs);
        }
        finally
        {
            isSaving = false;
        }
    }
}

file static class FlightTimeCalculatorClient
{
    public static (int BlockMinutes, int FlightMinutes) CalculateSectorTimes(
        DateTimeOffset blockOff, DateTimeOffset blockOn, DateTimeOffset? takeoff, DateTimeOffset? landing)
    {
        var blockMinutes = FlightTimeFormatting.RoundNearestMinute((blockOn - blockOff).TotalMinutes);
        if (blockMinutes < 0) blockMinutes = 0;
        var flightMinutes = takeoff.HasValue && landing.HasValue
            ? Math.Max(0, FlightTimeFormatting.RoundNearestMinute((landing.Value - takeoff.Value).TotalMinutes))
            : blockMinutes;
        return (blockMinutes, flightMinutes);
    }
}
