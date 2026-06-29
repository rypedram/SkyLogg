using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class CountriesPage : BaseInfoPageBase
{
    [AutoInject] private ICountryController countryController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? searchText;
    private string? formSnapshot;
    private CountryDto? countryPendingDelete;
    private CountryDto editingCountry = new();
    private List<CountryDto> countries = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await SearchCountries();
        CaptureFormSnapshot();
    }

    private async Task SearchCountries()
    {
        isLoading = true;
        try
        {
            PagedResponse<CountryDto> result = string.IsNullOrWhiteSpace(searchText)
                ? await countryController.GetCountries(CurrentCancellationToken)
                : await countryController.SearchCountries(searchText.Trim(), CurrentCancellationToken);

            countries = result.Items?.ToList() ?? [];
        }
        finally
        {
            isLoading = false;
        }
    }

    private void LoadCountry(CountryDto country)
    {
        editingCountry = new CountryDto
        {
            Id = country.Id,
            Name = country.Name,
            Iso2 = country.Iso2,
            Iso3 = country.Iso3,
        };
        CaptureFormSnapshot();
    }

    private void EditCountry(CountryDto country) => OpenEditForm(() => LoadCountry(country));

    private async Task SaveCountry()
    {
        isSaving = true;
        try
        {
            if (editingCountry.Id == Guid.Empty)
                await countryController.Create(editingCountry, CurrentCancellationToken);
            else
                await countryController.Update(editingCountry, CurrentCancellationToken);

            ResetFormInternal();
            CloseForm();
            await SearchCountries();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ConfirmDeleteCountry(CountryDto country)
    {
        countryPendingDelete = country;
        OpenDeleteDialog(country.DisplayName);
    }

    private async Task DeleteCountryConfirmed()
    {
        if (countryPendingDelete is null)
            return;

        await countryController.Delete(countryPendingDelete.Id, CurrentCancellationToken);
        countryPendingDelete = null;
        await SearchCountries();
    }

    private void RequestResetForm() => RequestCancel(HasUnsavedChanges(), ResetFormInternal);

    private void ResetFormInternal()
    {
        editingCountry = new CountryDto();
        CaptureFormSnapshot();
    }

    private void CaptureFormSnapshot() => formSnapshot = FormStateHelper.Capture(editingCountry);

    private bool HasUnsavedChanges() => FormStateHelper.HasChanges(editingCountry, formSnapshot);

    protected override void OnDeleteDialogCanceled() => countryPendingDelete = null;
}
