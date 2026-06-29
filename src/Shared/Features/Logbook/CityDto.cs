namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class CityDto : IArchivable
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(150)]
    [Display(Name = nameof(AppStrings.City))]
    public string? Name { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [Display(Name = nameof(AppStrings.Country))]
    public Guid CountryId { get; set; }

    [MaxLength(100)]
    public string? CountryName { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [Display(Name = nameof(AppStrings.TimeZone))]
    public Guid TimeZoneId { get; set; }

    [MaxLength(200)]
    public string? TimeZoneDisplay { get; set; }

    public bool IsArchived { get; set; }

    [JsonIgnore, NotMapped]
    public string DisplayName => string.IsNullOrWhiteSpace(CountryName)
        ? Name ?? ""
        : $"{Name}, {CountryName}";
}
