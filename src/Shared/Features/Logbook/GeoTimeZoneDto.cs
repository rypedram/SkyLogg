namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class GeoTimeZoneDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    [Display(Name = nameof(AppStrings.IanaTimeZoneId))]
    public string? IanaId { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(200)]
    [Display(Name = nameof(AppStrings.TimeZoneDisplayName))]
    public string? DisplayName { get; set; }

    [MaxLength(10)]
    [Display(Name = nameof(AppStrings.UtcOffset))]
    public string? UtcOffset { get; set; }

    public bool IsArchived { get; set; }

    [JsonIgnore, NotMapped]
    public string Label => string.IsNullOrWhiteSpace(UtcOffset)
        ? DisplayName ?? IanaId ?? ""
        : $"{DisplayName} ({UtcOffset})";
}
