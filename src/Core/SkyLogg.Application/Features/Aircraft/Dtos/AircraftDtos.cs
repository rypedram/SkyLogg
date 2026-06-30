namespace SkyLogg.Application.Features.Aircraft.Dtos;

public class AircraftWriteDto
{
    public string Registration { get; set; } = string.Empty;

    public Guid? AircraftTypeId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
}

public sealed class AircraftReadDto : AircraftWriteDto
{
    public Guid Id { get; set; }

    public long Version { get; set; }

    public bool IsArchived { get; set; }
}
