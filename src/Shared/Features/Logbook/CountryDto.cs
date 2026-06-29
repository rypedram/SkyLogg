namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class CountryDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(2)]
    public string? Iso2 { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(3)]
    public string? Iso3 { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    public string? Name { get; set; }

    [JsonIgnore, NotMapped]
    public string DisplayName => string.IsNullOrWhiteSpace(Iso2) ? Name ?? "" : $"{Name} ({Iso2})";
}
