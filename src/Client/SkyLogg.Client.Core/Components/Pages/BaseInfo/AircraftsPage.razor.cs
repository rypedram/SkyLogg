using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class AircraftsPage : BaseInfoPageBase
{
    [AutoInject] private IAircraftController aircraftController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? formSnapshot;
    private string? selectedAircraftTypeValue;
    private AircraftDto? aircraftPendingDelete;
    private AircraftDto editingAircraft = new();
    private List<AircraftDto> aircraft = [];
    private List<AircraftTypeDto> aircraftTypes = [];
    private List<BitDropdownItem<string>> aircraftTypeItems = [];

    private bool IsFormActive
    {
        get => ArchivableFormHelper.GetIsActive(editingAircraft);
        set => ArchivableFormHelper.SetIsActive(editingAircraft, value);
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
            aircraftTypes = await aircraftController.GetAircraftTypes(CurrentCancellationToken);
            aircraftTypeItems = aircraftTypes
                .Select(t => new BitDropdownItem<string> { Value = t.Id.ToString(), Text = t.DisplayName })
                .ToList();

            aircraft = (await aircraftController.Get(CurrentCancellationToken))
                .OrderByDescending(a => !a.IsArchived)
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
            IsArchived = item.IsArchived,
            Version = item.Version,
        };
        selectedAircraftTypeValue = editingAircraft.AircraftTypeId?.ToString();
        CaptureFormSnapshot();
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

            ResetFormInternal();
            await LoadData();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ConfirmDeleteAircraft(AircraftDto item)
    {
        aircraftPendingDelete = item;
        OpenDeleteDialog(item.Registration ?? string.Empty);
    }

    private async Task DeleteAircraftConfirmed()
    {
        if (aircraftPendingDelete is null)
            return;

        await aircraftController.Delete(aircraftPendingDelete.Id, aircraftPendingDelete.Version, CurrentCancellationToken);
        aircraftPendingDelete = null;
        await LoadData();
    }

    private void RequestResetForm() => RequestCancel(HasUnsavedChanges(), ResetFormInternal);

    private void ResetFormInternal()
    {
        editingAircraft = new AircraftDto();
        selectedAircraftTypeValue = null;
        CaptureFormSnapshot();
    }

    private AircraftFormSnapshot CurrentFormState() => new(editingAircraft, selectedAircraftTypeValue);

    private void CaptureFormSnapshot() => formSnapshot = FormStateHelper.Capture(CurrentFormState());

    private bool HasUnsavedChanges() => FormStateHelper.HasChanges(CurrentFormState(), formSnapshot);

    protected override void OnDeleteDialogCanceled() => aircraftPendingDelete = null;

    private record AircraftFormSnapshot(AircraftDto Aircraft, string? AircraftTypeValue);
}
