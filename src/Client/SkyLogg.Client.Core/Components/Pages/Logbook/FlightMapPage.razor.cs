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
    private string? loadError;
    private FlightMapDto? mapData;
    private List<BitDropdownItem<string>> aircraftItems = [];

    private IEnumerable<FlightMapRouteDto> MappableRoutes =>
        mapData?.Routes.Where(r => r.HasCoordinates) ?? [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        aircraftItems = [new() { Text = Localizer[nameof(AppStrings.All)], Value = "" }];

        try
        {
            var aircraft = await aircraftController.Get(CurrentCancellationToken);
            aircraftItems.AddRange(aircraft
                .Where(a => a.IsArchived is false)
                .OrderBy(a => a.Registration)
                .Select(a => new BitDropdownItem<string> { Text = a.Registration, Value = a.Id.ToString() }));
        }
        catch (KnownException e)
        {
            SnackBarService.Error(e.Message);
        }

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
        loadError = null;

        try
        {
            mapData = await flightMapController.GetMapData(
                null,
                null,
                aircraftFilter,
                GetEffectiveYearFilter(),
                GetEffectiveMonthFilter(),
                CurrentCancellationToken);
        }
        catch (KnownException e)
        {
            loadError = e.Message;
            mapData = null;
            SnackBarService.Error(e.Message);
        }
        catch (Exception)
        {
            loadError = Localizer[nameof(AppStrings.FlightMapLoadFailed)];
            mapData = null;
            SnackBarService.Error(loadError);
        }
        finally
        {
            isLoading = false;
        }
    }

    private int? GetEffectiveYearFilter() => yearFilter is >= 1900 and <= 2100 ? yearFilter : null;

    private int? GetEffectiveMonthFilter() => monthFilter is >= 1 and <= 12 ? monthFilter : null;

    private static IEnumerable<string> GetPolylineSegments(FlightMapRouteDto route)
    {
        if (route.HasCoordinates is false)
            yield break;

        var points = route.GreatCirclePoints.Count > 0
            ? route.GreatCirclePoints
            : [new FlightMapPointDto { Latitude = route.DepartureLatitude, Longitude = route.DepartureLongitude },
               new FlightMapPointDto { Latitude = route.ArrivalLatitude, Longitude = route.ArrivalLongitude }];

        var projected = points
            .Select(p => (X: ProjectX(p.Longitude), Y: ProjectY(p.Latitude), Longitude: p.Longitude))
            .ToList();

        if (projected.Count < 2)
            yield break;

        var segment = new List<(double X, double Y)>(projected.Count) { (projected[0].X, projected[0].Y) };

        for (var i = 1; i < projected.Count; i++)
        {
            if (Math.Abs(projected[i].Longitude - projected[i - 1].Longitude) > 180)
            {
                if (segment.Count >= 2)
                    yield return FormatPolyline(segment);

                segment = [(projected[i].X, projected[i].Y)];
                continue;
            }

            segment.Add((projected[i].X, projected[i].Y));
        }

        if (segment.Count >= 2)
            yield return FormatPolyline(segment);
    }

    private static string FormatPolyline(IReadOnlyList<(double X, double Y)> segment) =>
        string.Join(" ", segment.Select(p => $"{p.X:0.##},{p.Y:0.##}"));

    private static double ProjectX(double longitude) => longitude + 180;

    private static double ProjectY(double latitude) => 90 - latitude;

    private static double AirportRadius(int visitCount) => Math.Clamp(2 + visitCount, 3, 10);
}
