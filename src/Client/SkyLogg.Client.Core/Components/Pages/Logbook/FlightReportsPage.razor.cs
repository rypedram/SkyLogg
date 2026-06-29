using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightReportsPage
{
    [AutoInject] private IFlightReportsController flightReportsController = default!;

    private bool isLoading;
    private FlightStatisticsDto? statistics;
    private List<AchievementStatusDto> achievements = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadReports();
    }

    private async Task LoadReports()
    {
        isLoading = true;
        try
        {
            statistics = await flightReportsController.GetStatistics(CurrentCancellationToken);
            achievements = await flightReportsController.GetAchievements(CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string FormatFlightStatistic(FlightStatisticItemDto? item)
    {
        return item is null
            ? "-"
            : $"{item.FlightDate:d} — {item.Route} — {FlightTimeFormatting.FormatMinutes(item.FlightMinutes)}";
    }
}
