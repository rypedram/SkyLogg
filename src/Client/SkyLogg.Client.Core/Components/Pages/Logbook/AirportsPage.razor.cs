using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class AirportsPage
{
    [AutoInject] private IAirportController airportController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? searchText;
    private string? selectedCountryValue;
    private AirportDto editingAirport = new() { IsActive = true };
    private List<AirportDto> airports = [];
    private List<CountryDto> countries = [];
    private List<BitDropdownItem<string>> countryItems = [];

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
            countries = await airportController.GetCountries(CurrentCancellationToken);
            countryItems = countries
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

    private void OnCountryChanged(string? value)
    {
        selectedCountryValue = value;
        editingAirport.CountryId = Guid.TryParse(value, out var countryId) ? countryId : null;

        var country = countries.FirstOrDefault(c => c.Id == editingAirport.CountryId);
        if (country is not null)
            editingAirport.Country = country.Name;
    }

    private void EditAirport(AirportDto airport)
    {
        editingAirport = new AirportDto
        {
            Id = airport.Id,
            IATA = airport.IATA,
            ICAO = airport.ICAO,
            Name = airport.Name,
            CountryId = airport.CountryId,
            Country = airport.Country,
            City = airport.City,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            ElevationFt = airport.ElevationFt,
            IsActive = airport.IsActive,
        };
        selectedCountryValue = editingAirport.CountryId?.ToString();
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

            ResetForm();
            await SearchAirports();
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeactivateAirport(AirportDto airport)
    {
        await airportController.Delete(airport.Id, CurrentCancellationToken);
        await SearchAirports();
    }

    private void ResetForm()
    {
        editingAirport = new AirportDto { IsActive = true };
        selectedCountryValue = null;
    }
}
