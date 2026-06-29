namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class FlightLogCrewDto
{
    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public Guid CrewMemberId { get; set; }

    public string? CrewMemberName { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    public CrewRoleType RoleType { get; set; }
}
