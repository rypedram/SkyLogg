using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Logbook;

public partial class AirportPicker
{
    [Parameter] public Guid SelectedAirportId { get; set; }
    [Parameter] public EventCallback<Guid> SelectedAirportIdChanged { get; set; }

    [AutoInject] private IAirportController airportController = default!;

    private List<BitDropdownItem<string>> airportItems = [];
    private string? selectedValue;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadAirports(null);
        SyncSelectedValue();
    }

    protected override async Task OnParamsSetAsync()
    {
        await base.OnParamsSetAsync();
        SyncSelectedValue();
    }

    private void SyncSelectedValue()
    {
        selectedValue = SelectedAirportId == Guid.Empty ? null : SelectedAirportId.ToString();
    }

    private async Task LoadAirports(string? query)
    {
        PagedResponse<AirportDto> result = string.IsNullOrWhiteSpace(query)
            ? await airportController.GetAirports(CurrentCancellationToken)
            : await airportController.SearchAirports(query.Trim(), CurrentCancellationToken);

        airportItems = (result.Items ?? [])
            .Select(a => new BitDropdownItem<string> { Value = a.Id.ToString(), Text = a.DisplayName })
            .ToList();
    }

    private async Task OnSearchAirports(string? search)
    {
        await LoadAirports(search);
    }

    private void OnAirportChanged(string? airportId)
    {
        if (string.IsNullOrEmpty(airportId) || Guid.TryParse(airportId, out var id) is false)
            return;

        selectedValue = airportId;
        SelectedAirportId = id;
        _ = SelectedAirportIdChanged.InvokeAsync(id);
    }
}
