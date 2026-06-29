namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class AircraftDto : IArchivable
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(20)]
    [Display(Name = nameof(AppStrings.Registration))]
    public string? Registration { get; set; }

    public Guid? AircraftTypeId { get; set; }

    public string? AircraftTypeDisplay { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(50)]
    [Display(Name = nameof(AppStrings.AircraftType))]
    public string? Type { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    [Display(Name = nameof(AppStrings.AircraftModel))]
    public string? Model { get; set; }

    public bool IsArchived { get; set; }

    public long Version { get; set; }
}
