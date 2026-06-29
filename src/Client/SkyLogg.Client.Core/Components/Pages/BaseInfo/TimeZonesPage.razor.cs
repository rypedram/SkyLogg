using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class TimeZonesPage : BaseInfoPageBase
{
    [AutoInject] private IGeoTimeZoneController geoTimeZoneController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? searchText;
    private string? formSnapshot;
    private GeoTimeZoneDto? timeZonePendingDelete;
    private GeoTimeZoneDto editingTimeZone = new();
    private List<GeoTimeZoneDto> timeZones = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await SearchTimeZones();
        CaptureFormSnapshot();
    }

    private async Task SearchTimeZones()
    {
        isLoading = true;
        try
        {
            PagedResponse<GeoTimeZoneDto> result = string.IsNullOrWhiteSpace(searchText)
                ? await geoTimeZoneController.GetTimeZones(CurrentCancellationToken)
                : await geoTimeZoneController.SearchTimeZones(searchText.Trim(), CurrentCancellationToken);

            timeZones = result.Items?.ToList() ?? [];
        }
        finally
        {
            isLoading = false;
        }
    }

    private void LoadTimeZone(GeoTimeZoneDto timeZone)
    {
        editingTimeZone = new GeoTimeZoneDto
        {
            Id = timeZone.Id,
            IanaId = timeZone.IanaId,
            DisplayName = timeZone.DisplayName,
            UtcOffset = timeZone.UtcOffset,
        };
        CaptureFormSnapshot();
    }

    private void EditTimeZone(GeoTimeZoneDto timeZone) => OpenEditForm(() => LoadTimeZone(timeZone));

    private async Task SaveTimeZone()
    {
        isSaving = true;
        try
        {
            if (editingTimeZone.Id == Guid.Empty)
                await geoTimeZoneController.Create(editingTimeZone, CurrentCancellationToken);
            else
                await geoTimeZoneController.Update(editingTimeZone, CurrentCancellationToken);

            ResetFormInternal();
            CloseForm();
            await SearchTimeZones();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ConfirmDeleteTimeZone(GeoTimeZoneDto timeZone)
    {
        timeZonePendingDelete = timeZone;
        OpenDeleteDialog(timeZone.Label);
    }

    private async Task DeleteTimeZoneConfirmed()
    {
        if (timeZonePendingDelete is null)
            return;

        await geoTimeZoneController.Delete(timeZonePendingDelete.Id, CurrentCancellationToken);
        timeZonePendingDelete = null;
        await SearchTimeZones();
    }

    private void RequestResetForm() => RequestCancel(HasUnsavedChanges(), ResetFormInternal);

    private void ResetFormInternal()
    {
        editingTimeZone = new GeoTimeZoneDto();
        CaptureFormSnapshot();
    }

    private void CaptureFormSnapshot() => formSnapshot = FormStateHelper.Capture(editingTimeZone);

    private bool HasUnsavedChanges() => FormStateHelper.HasChanges(editingTimeZone, formSnapshot);

    protected override void OnDeleteDialogCanceled() => timeZonePendingDelete = null;
}
