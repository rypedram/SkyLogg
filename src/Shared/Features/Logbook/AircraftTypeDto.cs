namespace SkyLogg.Shared.Features.Logbook;

[DtoResourceType(typeof(AppStrings))]
public partial class AircraftTypeDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(100)]
    public string? Model { get; set; }

    [Required(ErrorMessage = nameof(AppStrings.RequiredAttribute_ValidationError))]
    [MaxLength(50)]
    public string? TypeCode { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? EngineType { get; set; }

    public bool IsArchived { get; set; }

    public long Version { get; set; }

    [JsonIgnore, NotMapped]
    public string DisplayName => string.Join(" ", new[] { Manufacturer, Model, TypeCode }.Where(value => string.IsNullOrWhiteSpace(value) is false));
}
