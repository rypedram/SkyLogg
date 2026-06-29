using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightLogsPage
{
    [AutoInject] private IFlightLogController flightLogController = default!;

    private bool isLoading;
    private string? searchText;
    private DateTimeOffset? filterFromDate;
    private DateTimeOffset? filterToDate;
    private List<FlightLogDto> allFlights = [];
    private List<FlightLogDto> filteredFlights = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadFlights();
    }

    private async Task LoadFlights()
    {
        isLoading = true;
        try
        {
            var paged = await flightLogController.GetFlightLogs(CurrentCancellationToken);
            allFlights = paged.Items?.OrderByDescending(f => f.FlightDate).ToList() ?? [];
            FilterFlights();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void FilterFlights()
    {
        IEnumerable<FlightLogDto> query = allFlights;

        if (string.IsNullOrWhiteSpace(searchText) is false)
        {
            var term = searchText.Trim();
            query = query.Where(f =>
                (f.AircraftRegistration?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (f.RouteSummary?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (f.Remarks?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (filterFromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(filterFromDate.Value.Date);
            query = query.Where(f => f.FlightDate >= from);
        }

        if (filterToDate.HasValue)
        {
            var to = DateOnly.FromDateTime(filterToDate.Value.Date);
            query = query.Where(f => f.FlightDate <= to);
        }

        filteredFlights = query.ToList();
    }
}
