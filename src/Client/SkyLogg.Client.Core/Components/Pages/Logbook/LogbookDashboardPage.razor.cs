using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class LogbookDashboardPage
{
    [AutoInject] private IFlightLogController flightLogController = default!;

    private bool isLoading;
    private CurrencyStatusDto? currency;
    private List<FlightLogDto> recentFlights = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            currency = await flightLogController.GetCurrencyStatus(CurrentCancellationToken);
            var paged = await flightLogController.GetFlightLogs(CurrentCancellationToken);
            recentFlights = paged.Items?.OrderByDescending(f => f.FlightDate).Take(5).ToList() ?? [];
        }
        finally
        {
            isLoading = false;
        }
    }
}
