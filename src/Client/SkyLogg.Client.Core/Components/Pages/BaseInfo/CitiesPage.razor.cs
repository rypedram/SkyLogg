using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class CitiesPage : BaseInfoPageBase
{
    [AutoInject] private ICityController cityController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? searchText;
    private string? formSnapshot;
    private string? selectedCountryValue;
    private string? selectedTimeZoneValue;
    private CityDto? cityPendingDelete;
    private CityDto editingCity = new();
    private List<CityDto> cities = [];
    private List<CountryDto> countries = [];
    private List<GeoTimeZoneDto> timeZones = [];
    private List<BitDropdownItem<string>> countryItems = [];
    private List<BitDropdownItem<string>> timeZoneItems = [];

    private bool IsFormActive
    {
        get => ArchivableFormHelper.GetIsActive(editingCity);
        set => ArchivableFormHelper.SetIsActive(editingCity, value);
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
            countries = await cityController.GetCountries(CurrentCancellationToken);
            timeZones = await cityController.GetTimeZones(CurrentCancellationToken);

            countryItems = countries
                .Select(c => new BitDropdownItem<string> { Value = c.Id.ToString(), Text = c.DisplayName })
                .ToList();
            timeZoneItems = timeZones
                .Select(t => new BitDropdownItem<string> { Value = t.Id.ToString(), Text = t.Label })
                .ToList();

            await SearchCities();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task SearchCities()
    {
        PagedResponse<CityDto> result = string.IsNullOrWhiteSpace(searchText)
            ? await cityController.GetCities(CurrentCancellationToken)
            : await cityController.SearchCities(searchText.Trim(), CurrentCancellationToken);

        cities = result.Items?.ToList() ?? [];
    }

    private void OnCountryChanged(string? value)
    {
        selectedCountryValue = value;
        editingCity.CountryId = Guid.TryParse(value, out var countryId) ? countryId : Guid.Empty;

        var country = countries.FirstOrDefault(c => c.Id == editingCity.CountryId);
        if (country is not null)
            editingCity.CountryName = country.Name;
    }

    private void OnTimeZoneChanged(string? value)
    {
        selectedTimeZoneValue = value;
        editingCity.TimeZoneId = Guid.TryParse(value, out var timeZoneId) ? timeZoneId : Guid.Empty;

        var timeZone = timeZones.FirstOrDefault(t => t.Id == editingCity.TimeZoneId);
        if (timeZone is not null)
            editingCity.TimeZoneDisplay = timeZone.Label;
    }

    private void EditCity(CityDto city)
    {
        editingCity = new CityDto
        {
            Id = city.Id,
            Name = city.Name,
            CountryId = city.CountryId,
            CountryName = city.CountryName,
            TimeZoneId = city.TimeZoneId,
            TimeZoneDisplay = city.TimeZoneDisplay,
            IsArchived = city.IsArchived,
        };
        selectedCountryValue = editingCity.CountryId.ToString();
        selectedTimeZoneValue = editingCity.TimeZoneId.ToString();
        CaptureFormSnapshot();
    }

    private async Task SaveCity()
    {
        isSaving = true;
        try
        {
            if (editingCity.Id == Guid.Empty)
                await cityController.Create(editingCity, CurrentCancellationToken);
            else
                await cityController.Update(editingCity, CurrentCancellationToken);

            ResetFormInternal();
            await SearchCities();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ConfirmDeleteCity(CityDto city)
    {
        cityPendingDelete = city;
        OpenDeleteDialog(city.DisplayName);
    }

    private async Task DeleteCityConfirmed()
    {
        if (cityPendingDelete is null)
            return;

        await cityController.Delete(cityPendingDelete.Id, CurrentCancellationToken);
        cityPendingDelete = null;
        await SearchCities();
    }

    private void RequestResetForm() => RequestCancel(HasUnsavedChanges(), ResetFormInternal);

    private void ResetFormInternal()
    {
        editingCity = new CityDto();
        selectedCountryValue = null;
        selectedTimeZoneValue = null;
        CaptureFormSnapshot();
    }

    private CityFormSnapshot CurrentFormState() => new(editingCity, selectedCountryValue, selectedTimeZoneValue);

    private void CaptureFormSnapshot() => formSnapshot = FormStateHelper.Capture(CurrentFormState());

    private bool HasUnsavedChanges() => FormStateHelper.HasChanges(CurrentFormState(), formSnapshot);

    protected override void OnDeleteDialogCanceled() => cityPendingDelete = null;

    private record CityFormSnapshot(CityDto City, string? CountryValue, string? TimeZoneValue);
}
