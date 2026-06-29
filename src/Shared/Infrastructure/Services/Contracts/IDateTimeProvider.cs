namespace SkyLogg.Shared.Infrastructure.Services.Contracts;

public interface IDateTimeProvider
{
    DateTimeOffset GetCurrentDateTime();
}
