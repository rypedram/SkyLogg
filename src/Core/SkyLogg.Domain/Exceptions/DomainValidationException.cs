namespace SkyLogg.Domain.Exceptions;

public sealed class DomainValidationException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public DomainValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }

    public DomainValidationException(string error) : base(error)
    {
        Errors = [error];
    }
}
