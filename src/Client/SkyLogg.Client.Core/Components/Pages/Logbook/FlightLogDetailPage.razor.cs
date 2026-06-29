using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightLogDetailPage
{
    [Parameter] public Guid Id { get; set; }

    [AutoInject] private IFlightLogController flightLogController = default!;

    private bool isLoading;
    private bool isDeleteDialogOpen;
    private FlightLogDto? flight;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadFlight();
    }

    private async Task LoadFlight()
    {
        isLoading = true;
        try
        {
            flight = await flightLogController.Get(Id, CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ConfirmDelete() => isDeleteDialogOpen = true;

    private async Task DeleteFlight()
    {
        if (flight is null) return;

        await flightLogController.Delete(flight.Id, flight.Version, CurrentCancellationToken);
        NavigationManager.NavigateTo(PageUrls.FlightLogs);
    }
}
