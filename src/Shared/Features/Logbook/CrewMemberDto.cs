namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class CrewMemberDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(200)]
    [Display(Name = nameof(AppStrings.CrewName))]
    public string? Name { get; set; }

    [Display(Name = nameof(AppStrings.CrewRole))]
    public CrewRoleType? DefaultRole { get; set; }

    public long Version { get; set; }
}
