using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class FlightMapPage
{
    [AutoInject] private IFlightMapController flightMapController = default!;
    [AutoInject] private IAircraftController aircraftController = default!;

    private bool isLoading;
    private int? yearFilter;
    private int? monthFilter;
    private Guid? aircraftFilter;
    private string? aircraftFilterValue;
    private FlightMapDto? mapData;
    private List<BitDropdownItem<string>> aircraftItems = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        var aircraft = await aircraftController.Get(CurrentCancellationToken);
        aircraftItems = [new() { Text = Localizer[nameof(AppStrings.All)], Value = "" }];
        aircraftItems.AddRange(aircraft
            .Where(a => a.IsActive)
            .OrderBy(a => a.Registration)
            .Select(a => new BitDropdownItem<string> { Text = a.Registration, Value = a.Id.ToString() }));

        await LoadMap();
    }

    private void OnAircraftFilterChanged(string? value)
    {
        aircraftFilterValue = value;
        aircraftFilter = Guid.TryParse(value, out var aircraftId) ? aircraftId : null;
    }

    private async Task LoadMap()
    {
        isLoading = true;
        try
        {
            mapData = await flightMapController.GetMapData(null, null, aircraftFilter, yearFilter, monthFilter, CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string GetPolylinePoints(FlightMapRouteDto route)
    {
        var points = route.GreatCirclePoints.Count > 0
            ? route.GreatCirclePoints
            : [new FlightMapPointDto { Latitude = route.DepartureLatitude, Longitude = route.DepartureLongitude },
               new FlightMapPointDto { Latitude = route.ArrivalLatitude, Longitude = route.ArrivalLongitude }];

        return string.Join(" ", points.Select(p => $"{ProjectX(p.Longitude):0.##},{ProjectY(p.Latitude):0.##}"));
    }

    private static double ProjectX(double longitude) => (longitude + 180) * 360 / 360;

    private static double ProjectY(double latitude) => (90 - latitude) * 180 / 180;

    private static double AirportRadius(int visitCount) => Math.Clamp(2 + visitCount, 3, 10);
}
