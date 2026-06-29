using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class CrewMembersPage : BaseInfoPageBase
{
    [AutoInject] private ICrewMemberController crewMemberController = default!;

    private bool isLoading;
    private bool isSaving;
    private string? formSnapshot;
    private DateTimeOffset? birthdayDate;
    private string? selectedPositionTypeValue;
    private string? selectedCrewRoleValue;
    private CrewMemberDto? crewPendingDelete;
    private CrewMemberDto editingCrew = new();
    private List<CrewMemberDto> crewMembers = [];
    private List<BitDropdownItem<string>> positionTypeItems = [];
    private List<BitDropdownItem<string>> crewRoleItems = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        positionTypeItems = Enum.GetValues<CrewPositionType>()
            .Select(t => new BitDropdownItem<string> { Value = ((byte)t).ToString(), Text = t.ToString() })
            .ToList();
        crewRoleItems = Enum.GetValues<CrewRoleType>()
            .Select(t => new BitDropdownItem<string> { Value = ((byte)t).ToString(), Text = t.ToString() })
            .ToList();
        await LoadCrew();
        CaptureFormSnapshot();
    }

    private async Task LoadCrew()
    {
        isLoading = true;
        try
        {
            crewMembers = await crewMemberController.Get(CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OnPositionTypeChanged(string? value)
    {
        selectedPositionTypeValue = value;
        editingCrew.PositionType = byte.TryParse(value, out var positionType)
            ? (CrewPositionType)positionType
            : null;
    }

    private void OnCrewRoleChanged(string? value)
    {
        selectedCrewRoleValue = value;
        editingCrew.DefaultRole = byte.TryParse(value, out var role)
            ? (CrewRoleType)role
            : null;
    }

    private void LoadCrew(CrewMemberDto crew)
    {
        editingCrew = new CrewMemberDto
        {
            Id = crew.Id,
            FirstName = crew.FirstName,
            LastName = crew.LastName,
            LicenceNumber = crew.LicenceNumber,
            Birthday = crew.Birthday,
            PositionType = crew.PositionType,
            DefaultRole = crew.DefaultRole,
            IsArchived = crew.IsArchived,
            Version = crew.Version,
        };
        birthdayDate = crew.Birthday.HasValue
            ? new DateTimeOffset(crew.Birthday.Value.ToDateTime(TimeOnly.MinValue))
            : null;
        selectedPositionTypeValue = crew.PositionType.HasValue ? ((byte)crew.PositionType.Value).ToString() : null;
        selectedCrewRoleValue = crew.DefaultRole.HasValue ? ((byte)crew.DefaultRole.Value).ToString() : null;
        CaptureFormSnapshot();
    }

    private void EditCrew(CrewMemberDto crew) => OpenEditForm(() => LoadCrew(crew));

    private async Task SaveCrew()
    {
        editingCrew.Birthday = birthdayDate.HasValue
            ? DateOnly.FromDateTime(birthdayDate.Value.Date)
            : null;

        isSaving = true;
        try
        {
            if (editingCrew.Id == Guid.Empty)
                await crewMemberController.Create(editingCrew, CurrentCancellationToken);
            else
                await crewMemberController.Update(editingCrew, CurrentCancellationToken);

            ResetFormInternal();
            CloseForm();
            await LoadCrew();
        }
        finally
        {
            isSaving = false;
        }
    }

    private void ConfirmDeleteCrew(CrewMemberDto crew)
    {
        crewPendingDelete = crew;
        OpenDeleteDialog(crew.FullName ?? string.Empty);
    }

    private async Task DeleteCrewConfirmed()
    {
        if (crewPendingDelete is null)
            return;

        await crewMemberController.Delete(crewPendingDelete.Id, crewPendingDelete.Version, CurrentCancellationToken);
        crewPendingDelete = null;
        await LoadCrew();
    }

    private void RequestResetForm() => RequestCancel(HasUnsavedChanges(), ResetFormInternal);

    private void ResetFormInternal()
    {
        editingCrew = new CrewMemberDto();
        birthdayDate = null;
        selectedPositionTypeValue = null;
        selectedCrewRoleValue = null;
        CaptureFormSnapshot();
    }

    private CrewFormSnapshot CurrentFormState() => new(editingCrew, birthdayDate, selectedPositionTypeValue, selectedCrewRoleValue);

    private void CaptureFormSnapshot() => formSnapshot = FormStateHelper.Capture(CurrentFormState());

    private bool HasUnsavedChanges() => FormStateHelper.HasChanges(CurrentFormState(), formSnapshot);

    protected override void OnDeleteDialogCanceled() => crewPendingDelete = null;

    private record CrewFormSnapshot(CrewMemberDto Crew, DateTimeOffset? Birthday, string? PositionTypeValue, string? CrewRoleValue);
}
