using SkyLogg.Domain.Exceptions;

namespace SkyLogg.Domain.ValueObjects;

public readonly record struct AirportCode
{
    public string Value { get; }

    public bool IsIcao => Value.Length == 4;

    public bool IsIata => Value.Length == 3;

    private AirportCode(string value) => Value = value;

    public static AirportCode Create(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainValidationException("Airport code is required.");

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length is not (3 or 4))
            throw new DomainValidationException("Airport code must be 3 (IATA) or 4 (ICAO) characters.");

        if (!normalized.All(char.IsLetterOrDigit))
            throw new DomainValidationException("Airport code contains invalid characters.");

        return new AirportCode(normalized);
    }

    public override string ToString() => Value;
}
