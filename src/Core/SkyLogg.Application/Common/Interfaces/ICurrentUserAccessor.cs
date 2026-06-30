namespace SkyLogg.Application.Common.Interfaces;

public interface ICurrentUserAccessor
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}
