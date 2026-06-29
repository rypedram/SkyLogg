namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class CrewMemberDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    [Display(Name = nameof(AppStrings.FirstName))]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    [Display(Name = nameof(AppStrings.LastName))]
    public string? LastName { get; set; }

    [MaxLength(50)]
    [Display(Name = nameof(AppStrings.LicenceNumber))]
    public string? LicenceNumber { get; set; }

    [Display(Name = nameof(AppStrings.Birthday))]
    public DateOnly? Birthday { get; set; }

    [Display(Name = nameof(AppStrings.CrewPositionType))]
    public CrewPositionType? PositionType { get; set; }

    [Display(Name = nameof(AppStrings.CrewRole))]
    public CrewRoleType? DefaultRole { get; set; }

    public bool IsArchived { get; set; }

    public long Version { get; set; }

    [JsonIgnore, NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
