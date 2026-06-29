using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class AirportsPage : BaseInfoPageBase
{
    [AutoInject] private IAirportController airportController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? searchText;
    private string? formSnapshot;
    private string? selectedCityValue;
    private AirportDto? airportPendingDelete;
    private AirportDto editingAirport = new();
    private List<AirportDto> airports = [];
    private List<CityDto> cities = [];
    private List<BitDropdownItem<string>> cityItems = [];

    private bool IsFormActive
    {
        get => ArchivableFormHelper.GetIsActive(editingAirport);
        set => ArchivableFormHelper.SetIsActive(editingAirport, value);
    }

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadData();
        CaptureFormSnapshot();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            cities = await airportController.GetCities(CurrentCancellationToken);
            cityItems = cities
                .Select(c => new BitDropdownItem<string> { Value = c.Id.ToString(), Text = c.DisplayName })
                .ToList();

            await SearchAirports();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task SearchAirports()
    {
        PagedResponse<AirportDto> result = string.IsNullOrWhiteSpace(searchText)
            ? await airportController.GetAirports(CurrentCancellationToken)
            : await airportController.SearchAirports(searchText.Trim(), CurrentCancellationToken);

        airports = result.Items?.ToList() ?? [];
    }

    private void OnCityChanged(string? value)
    {
        selectedCityValue = value;
        editingAirport.CityId = Guid.TryParse(value, out var cityId) ? cityId : Guid.Empty;

        var city = cities.FirstOrDefault(c => c.Id == editingAirport.CityId);
        if (city is null)
            return;

        editingAirport.City = city.Name;
        editingAirport.CountryId = city.CountryId;
        editingAirport.Country = city.CountryName;
        editingAirport.TimeZoneDisplay = city.TimeZoneDisplay;
    }

    private void EditAirport(AirportDto airport)
    {
        editingAirport = new AirportDto
        {
            Id = airport.Id,
            IATA = airport.IATA,
            ICAO = airport.ICAO,
            Name = airport.Name,
            CityId = airport.CityId,
            City = airport.City,
            CountryId = airport.CountryId,
            Country = airport.Country,
            TimeZoneDisplay = airport.TimeZoneDisplay,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            ElevationFt = airport.ElevationFt,
            IsArchived = airport.IsArchived,
        };
        selectedCityValue = editingAirport.CityId.ToString();
        CaptureFormSnapshot();
    }

    private async Task SaveAirport()
    {
        isSaving = true;
        try
        {
            if (editingAirport.Id == Guid.Empty)
                await airportController.Create(editingAirport, CurrentCancellationToken);
            else
                await airportController.Update(editingAirport, CurrentCancellationToken);

            ResetFormInternal();
            await SearchAirports();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ConfirmDeleteAirport(AirportDto airport)
    {
        airportPendingDelete = airport;
        OpenDeleteDialog(airport.DisplayName);
    }

    private async Task DeleteAirportConfirmed()
    {
        if (airportPendingDelete is null)
            return;

        await airportController.Delete(airportPendingDelete.Id, CurrentCancellationToken);
        airportPendingDelete = null;
        await SearchAirports();
    }

    private void RequestResetForm() => RequestCancel(HasUnsavedChanges(), ResetFormInternal);

    private void ResetFormInternal()
    {
        editingAirport = new AirportDto();
        selectedCityValue = null;
        CaptureFormSnapshot();
    }

    private AirportFormSnapshot CurrentFormState() => new(editingAirport, selectedCityValue);

    private void CaptureFormSnapshot() => formSnapshot = FormStateHelper.Capture(CurrentFormState());

    private bool HasUnsavedChanges() => FormStateHelper.HasChanges(CurrentFormState(), formSnapshot);

    protected override void OnDeleteDialogCanceled() => airportPendingDelete = null;

    private record AirportFormSnapshot(AirportDto Airport, string? CityValue);
}
