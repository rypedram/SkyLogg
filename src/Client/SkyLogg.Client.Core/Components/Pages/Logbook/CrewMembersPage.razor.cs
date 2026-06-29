using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.Logbook;

public partial class CrewMembersPage
{
    [AutoInject] private ICrewMemberController crewMemberController = default!;

    private string? newCrewName;
    private List<CrewMemberDto> crewMembers = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        await LoadCrew();
    }

    private async Task LoadCrew()
    {
        crewMembers = await crewMemberController.Get(CurrentCancellationToken);
    }

    private async Task AddCrew()
    {
        if (string.IsNullOrWhiteSpace(newCrewName)) return;

        await crewMemberController.Create(new CrewMemberDto { Name = newCrewName.Trim() }, CurrentCancellationToken);
        newCrewName = null;
        await LoadCrew();
    }

    private async Task DeleteCrew(CrewMemberDto crew)
    {
        await crewMemberController.Delete(crew.Id, crew.Version, CurrentCancellationToken);
        await LoadCrew();
    }
}
