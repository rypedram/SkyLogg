using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class AircraftsPage
{
    [AutoInject] private IAircraftController aircraftController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? selectedAircraftTypeValue;
    private AircraftDto editingAircraft = new();
    private List<AircraftDto> aircraft = [];
    private List<AircraftTypeDto> aircraftTypes = [];
    private List<BitDropdownItem<string>> aircraftTypeItems = [];

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
            aircraftTypes = await aircraftController.GetAircraftTypes(CurrentCancellationToken);
            aircraftTypeItems = aircraftTypes
                .Select(t => new BitDropdownItem<string> { Value = t.Id.ToString(), Text = t.DisplayName })
                .ToList();

            aircraft = (await aircraftController.Get(CurrentCancellationToken))
                .OrderByDescending(a => a.IsActive)
                .ThenBy(a => a.Registration)
                .ToList();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OnAircraftTypeChanged(string? value)
    {
        selectedAircraftTypeValue = value;
        editingAircraft.AircraftTypeId = Guid.TryParse(value, out var aircraftTypeId) ? aircraftTypeId : null;

        var aircraftType = aircraftTypes.FirstOrDefault(t => t.Id == editingAircraft.AircraftTypeId);
        if (aircraftType is null)
            return;

        editingAircraft.Type = aircraftType.Category ?? aircraftType.TypeCode;
        editingAircraft.Model = aircraftType.TypeCode;
        editingAircraft.AircraftTypeDisplay = aircraftType.DisplayName;
    }

    private void EditAircraft(AircraftDto item)
    {
        editingAircraft = new AircraftDto
        {
            Id = item.Id,
            Registration = item.Registration,
            AircraftTypeId = item.AircraftTypeId,
            AircraftTypeDisplay = item.AircraftTypeDisplay,
            Type = item.Type,
            Model = item.Model,
            IsActive = item.IsActive,
            Version = item.Version,
        };
        selectedAircraftTypeValue = editingAircraft.AircraftTypeId?.ToString();
    }

    private async Task SaveAircraft()
    {
        isSaving = true;
        try
        {
            if (editingAircraft.Id == Guid.Empty)
                await aircraftController.Create(editingAircraft, CurrentCancellationToken);
            else
                await aircraftController.Update(editingAircraft, CurrentCancellationToken);

            ResetForm();
            await LoadData();
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeactivateAircraft(AircraftDto item)
    {
        await aircraftController.Delete(item.Id, item.Version, CurrentCancellationToken);
        await LoadData();
    }

    private void ResetForm()
    {
        editingAircraft = new AircraftDto { IsActive = true };
        selectedAircraftTypeValue = null;
    }
}
